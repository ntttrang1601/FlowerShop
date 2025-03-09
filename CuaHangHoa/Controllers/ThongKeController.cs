using CuaHangHoa.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.IO;

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ThongKeController : Controller
    {
        private readonly MyDbContext _context;
        public ThongKeController(MyDbContext context)
        {
            _context = context;
            
        }


        public IActionResult Index(int? year)
        {
            // Lấy danh sách các năm có trong dữ liệu
            var years = _context.DonHangs
                .Select(dh => dh.NgayTao.Year)
                .Union(_context.PhieuXuats.Select(px => px.NgayXuat.Year))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Nếu không có năm được chọn, mặc định là năm hiện tại
            int selectedYear = year ?? DateTime.Now.Year;
            ViewBag.Years = new SelectList(years, selectedYear);  // Dùng SelectList để binding
            ViewBag.SelectedYear = selectedYear;

            var doanhThuDonHang = _context.DonHangs
                .Where(dh => dh.DaThanhToan && dh.NgayTao.Year == selectedYear)
                .GroupBy(dh => new { dh.NgayTao.Year, dh.NgayTao.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TongTien = g.Sum(dh => dh.TongTien)
                });

            var doanhThuPhieuXuat = _context.PhieuXuats
                .Where(px => px.NgayXuat.Year == selectedYear)
                .GroupBy(px => new { px.NgayXuat.Year, px.NgayXuat.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TongTien = g.Sum(px => px.TongTien)
                });

            var doanhThuTongHop = doanhThuDonHang
                .Concat(doanhThuPhieuXuat)
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Thang = $"{g.Key.Month}/{g.Key.Year}",
                    TongDoanhThu = g.Sum(x => x.TongTien)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            // Tổng doanh thu từ đơn hàng
            decimal totalRevenueDonHang = _context.DonHangs
                .Where(dh => dh.DaThanhToan && dh.NgayTao.Year == selectedYear)
                .Sum(dh => (decimal?)dh.TongTien) ?? 0;

            // Tổng doanh thu từ phiếu xuất
            decimal totalRevenuePhieuXuat = _context.PhieuXuats
                .Where(px => px.NgayXuat.Year == selectedYear)
                .Sum(px => (decimal?)px.TongTien) ?? 0;

            // Tổng doanh thu
            ViewBag.TotalRevenue = totalRevenueDonHang + totalRevenuePhieuXuat;

            // Tổng số đơn bán được
            ViewBag.TotalOrders = _context.DonHangs
                .Where(dh => dh.DaThanhToan == true && dh.NgayTao.Year == selectedYear)
                .Count();

            // Tổng số đăng ký dịch vụ
            ViewBag.TotalServices = _context.DangKyDichVus
                .Where(dv => dv.NgayDangKy.Year == selectedYear && dv.TrangThaiDangKy != Models.TrangThaiDK.DaHuy)
                .Count();

            // Lấy ID role "User"
            var userRoleId = _context.Roles.FirstOrDefault(r => r.Name == "User")?.Id;

            // Tổng số khách hàng với role là "User"
            ViewBag.TotalCustomers = _context.UserRoles
                .Where(ur => ur.RoleId == userRoleId)
                .Select(ur => ur.UserId)
                .Distinct()
                .Count();
            // Lấy 3 sản phẩm bán chạy nhất theo năm được chọn dựa trên tổng số lượng bán ra
            var topProducts = _context.DonHangs
                .Where(dh => dh.DaThanhToan && dh.NgayTao.Year == selectedYear) // Lọc theo năm và đơn đã thanh toán
                .SelectMany(dh => dh.ChiTietDHs) // Lấy tất cả chi tiết đơn hàng của các đơn
                .GroupBy(ct => ct.SanPhamId) // Nhóm theo ID sản phẩm
                .Select(g => new
                {
                    SanPhamId = g.Key,
                    TongSoLuong = g.Sum(ct => ct.SoLuong) // Tính tổng số lượng cho từng sản phẩm
                })
                .OrderByDescending(x => x.TongSoLuong) // Sắp xếp giảm dần theo số lượng bán
                .Take(3) // Lấy 3 sản phẩm bán chạy nhất
                .ToList();

            // Lấy tên và thông tin sản phẩm từ bảng SanPhams
            var topProductDetails = topProducts.Select(tp => new
            {
                ProductName = _context.SanPhams.FirstOrDefault(sp => sp.Id == tp.SanPhamId)?.Ten,
                TotalQuantity = tp.TongSoLuong
            }).ToList();

            ViewBag.TopProductDetails = topProductDetails;


            return View(doanhThuTongHop);
        }

        public IActionResult ThongKeSanPham(int? year, int? month)
        {
            // Lấy danh sách các năm có trong dữ liệu
            var years = _context.DonHangs
                .Select(dh => dh.NgayTao.Year)
                .Union(_context.PhieuXuats.Select(px => px.NgayXuat.Year))
                .Distinct()
                .OrderBy(y => y)
                .ToList();
            ViewBag.Years = new SelectList(years);

            // Khởi tạo danh sách tháng (rỗng nếu chưa chọn năm)
            if (year.HasValue)
            {
                var months = _context.DonHangs
                    .Where(dh => dh.NgayTao.Year == year)
                    .Select(dh => dh.NgayTao.Month)
                    .Union(_context.PhieuXuats
                        .Where(px => px.NgayXuat.Year == year)
                        .Select(px => px.NgayXuat.Month))
                    .Distinct()
                    .OrderBy(m => m)
                    .ToList();
                ViewBag.Months = new SelectList(months);
            }
            else
            {
                ViewBag.Months = new SelectList(Enumerable.Empty<int>());
            }


            // Tìm 5 sản phẩm bán chạy nếu chọn cả tháng và năm
            if (year.HasValue && month.HasValue)
            {
                var topProductsQuery = _context.DonHangs
                    .Where(dh => dh.DaThanhToan && dh.NgayTao.Year == year && dh.NgayTao.Month == month)
                    .SelectMany(dh => dh.ChiTietDHs)
                    .GroupBy(ct => ct.SanPhamId)
                    .Select(g => new
                    {
                        SanPhamId = g.Key,
                        TongSoLuong = g.Sum(ct => ct.SoLuong)
                    })
                    .OrderByDescending(x => x.TongSoLuong)
                    .Take(5)
                    .ToList();

                var productIds = topProductsQuery.Select(tp => tp.SanPhamId).ToList();
                var productNames = _context.SanPhams
                    .Where(sp => productIds.Contains(sp.Id))
                    .ToDictionary(sp => sp.Id, sp => sp.Ten);

                ViewBag.TopProducts = topProductsQuery
                    .Select(tp => new
                    {
                        ProductName = productNames.ContainsKey(tp.SanPhamId) ? productNames[tp.SanPhamId] : "Unknown",
                        TotalSold = tp.TongSoLuong
                    })
                    .ToList();
            }
            else
            {
                ViewBag.TopProducts = null;
            }

            return View();
        }
        public JsonResult GetMonths(int year)
        {
            var months = _context.DonHangs
                .Where(dh => dh.NgayTao.Year == year)
                .Select(dh => dh.NgayTao.Month)
                .Union(_context.PhieuXuats
                    .Where(px => px.NgayXuat.Year == year)
                    .Select(px => px.NgayXuat.Month))
                .Distinct()
                .OrderBy(m => m)
                .Select(m => new { Value = m, Text = "Tháng " + m })
                .ToList();

            return Json(new { months });
        }




        [HttpGet]
        public JsonResult GetTopProducts(int year, int month)
        {
            var topProductsQuery = _context.DonHangs
                .Where(dh => dh.DaThanhToan && dh.NgayTao.Year == year && dh.NgayTao.Month == month)
                .SelectMany(dh => dh.ChiTietDHs)
                .GroupBy(ct => ct.SanPhamId)
                .Select(g => new
                {
                    SanPhamId = g.Key,
                    TongSoLuong = g.Sum(ct => ct.SoLuong)
                })
                .OrderByDescending(x => x.TongSoLuong)
                .Take(5);

            var productIds = topProductsQuery.Select(tp => tp.SanPhamId).ToList();
            var productNames = _context.SanPhams
                .Where(sp => productIds.Contains(sp.Id))
                .ToDictionary(sp => sp.Id, sp => sp.Ten);

            var topProducts = topProductsQuery
                .Select(tp => new
                {
                    ProductName = productNames.ContainsKey(tp.SanPhamId) ? productNames[tp.SanPhamId] : "Unknown",
                    TotalSold = tp.TongSoLuong
                })
                .ToList();

            return Json(new { topProducts });
        }




        public IActionResult LoiNhuanTheoThang(int? year)
        {
            // Lấy danh sách các năm có trong dữ liệu
            var years = _context.DonHangs
                .Select(dh => dh.NgayTao.Year)
                .Union(_context.PhieuXuats.Select(px => px.NgayXuat.Year))
                .Union(_context.PhieuNhaps.Select(pn => pn.NgayNhap.Year))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Nếu không có năm được chọn, mặc định là năm hiện tại
            int selectedYear = year ?? DateTime.Now.Year;
            ViewBag.Years = new SelectList(years, selectedYear);
            ViewBag.SelectedYear = selectedYear;

            // Tạo danh sách tất cả các tháng trong năm
            var allMonths = Enumerable.Range(1, 12).Select(m => new { Month = m });

            // Tổng doanh thu (Đơn hàng: trừ phí vận chuyển)
            var doanhThuDonHangs = _context.DonHangs
                .Where(dh => dh.NgayTao.Year == selectedYear && dh.DaThanhToan)
                .GroupBy(dh => dh.NgayTao.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TongDoanhThu = g.Sum(dh => dh.TongTien - dh.PhiVanChuyen)
                });

            // Tổng doanh thu (Phiếu xuất: trừ phí phát sinh)
            var doanhThuPhieuXuats = _context.PhieuXuats
                .Where(px => px.NgayXuat.Year == selectedYear)
                .GroupBy(px => px.NgayXuat.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TongDoanhThu = g.Sum(px => px.TongTien - px.PhiPhatSinh)
                });

            // Tổng hợp doanh thu (Đơn hàng + Phiếu xuất)
            var doanhThuTongHop = doanhThuDonHangs
                .Union(doanhThuPhieuXuats)
                .GroupBy(x => x.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TongDoanhThu = g.Sum(x => x.TongDoanhThu)
                })
                .ToList(); // Chuyển kết quả về danh sách trong bộ nhớ

            // Tổng chi phí nhập hàng
            var tongChiPhiNhap = _context.PhieuNhaps
                .Where(pn => pn.NgayNhap.Year == selectedYear)
                .GroupBy(pn => pn.NgayNhap.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    TongChiPhiNhap = g.Sum(pn => pn.TongTien)
                })
                .ToList(); // Chuyển kết quả về danh sách trong bộ nhớ

            // Ghép doanh thu và chi phí nhập với danh sách tất cả các tháng
            var loiNhuanTheoThang = allMonths
                .GroupJoin(
                    doanhThuTongHop,
                    m => m.Month,
                    dt => dt.Month,
                    (m, dt) => new { Month = m.Month, DoanhThu = dt.FirstOrDefault()?.TongDoanhThu ?? 0 }
                )
                .GroupJoin(
                    tongChiPhiNhap,
                    m => m.Month,
                    pn => pn.Month,
                    (m, pn) => new
                    {
                        Month = m.Month,
                        TongDoanhThu = m.DoanhThu,
                        TongChiPhiNhap = pn.FirstOrDefault()?.TongChiPhiNhap ?? 0,
                        LoiNhuan = m.DoanhThu - (pn.FirstOrDefault()?.TongChiPhiNhap ?? 0)
                    }
                )
                .OrderBy(x => x.Month)
                .ToList(); // Chuyển kết quả về danh sách trong bộ nhớ

            return View(loiNhuanTheoThang);
        }


        public IActionResult ExportToExcel(int? year)
        {
            // Lấy dữ liệu thống kê dựa trên năm
            int selectedYear = year ?? DateTime.Now.Year;

            // Truy vấn số lượt đăng ký dịch vụ trong năm
            var soLuotDangKyDichVu = _context.DangKyDichVus
                .Where(dv => dv.NgayDangKy.Year == selectedYear && dv.TrangThaiDangKy != Models.TrangThaiDK.DaHuy)
                .GroupBy(dv => new { dv.NgayDangKy.Year, dv.NgayDangKy.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    SoLuotDangKy = g.Count() // Số lượt đăng ký dịch vụ trong tháng
                })
                .ToList();

            // Thống kê doanh thu đơn hàng (chỉ tính từ DonHangs)
            var doanhThuDonHang = _context.DonHangs
                .Where(dh => dh.DaThanhToan && dh.NgayTao.Year == selectedYear)
                .GroupBy(dh => new { dh.NgayTao.Year, dh.NgayTao.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TongTien = g.Sum(dh => dh.TongTien), // Tổng doanh thu từ đơn hàng
                    SoDonHang = g.Count() // Số đơn hàng trong tháng
                })
                .ToList();

            // Thống kê doanh thu phiếu xuất (Không tính số đơn hàng ở đây)
            var doanhThuPhieuXuat = _context.PhieuXuats
                .Where(px => px.NgayXuat.Year == selectedYear)
                .GroupBy(px => new { px.NgayXuat.Year, px.NgayXuat.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TongTien = g.Sum(px => px.TongTien), // Tổng doanh thu từ phiếu xuất
                    SoDonHang = 0 // Không tính số đơn hàng từ phiếu xuất
                })
                .ToList();

            // Tổng hợp dữ liệu: Kết hợp doanh thu từ DonHangs và PhieuXuats (chỉ tính số đơn từ DonHangs)
            var doanhThuTongHop = doanhThuDonHang
                .Concat(doanhThuPhieuXuat)
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Thang = $"{g.Key.Month}/{g.Key.Year}",
                    TongDoanhThu = g.Sum(x => x.TongTien), // Tổng doanh thu từ cả DonHangs và PhieuXuats
                    SoDonHang = g.Sum(x => x.SoDonHang), // Tổng số đơn hàng (chỉ từ DonHangs)
                    SoLuotDangKy = soLuotDangKyDichVu
                                    .FirstOrDefault(x => x.Year == g.Key.Year && x.Month == g.Key.Month)?.SoLuotDangKy ?? 0 // Lấy số lượt đăng ký dịch vụ
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            // Tính tổng cộng cho toàn bộ các cột
            var tongCong = new
            {
                Thang = "Tổng cộng",
                TongDoanhThu = doanhThuTongHop.Sum(x => x.TongDoanhThu),
                SoDonHang = doanhThuTongHop.Sum(x => x.SoDonHang),
                SoLuotDangKy = doanhThuTongHop.Sum(x => x.SoLuotDangKy) // Tổng số lượt đăng ký dịch vụ
            };

            // Tạo file Excel
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("DoanhThu");

                // Tạo header
                worksheet.Cells[1, 1].Value = "Tháng";
                worksheet.Cells[1, 2].Value = "Doanh thu (VND)";
                worksheet.Cells[1, 3].Value = "Số đơn hàng";
                worksheet.Cells[1, 4].Value = "Số lượt đăng ký dịch vụ"; // Cột số lượt đăng ký dịch vụ

                using (var range = worksheet.Cells[1, 1, 1, 4])
                {
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                // Thêm dữ liệu vào Excel
                for (int i = 0; i < doanhThuTongHop.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = doanhThuTongHop[i].Thang;
                    worksheet.Cells[i + 2, 2].Value = doanhThuTongHop[i].TongDoanhThu;
                    worksheet.Cells[i + 2, 3].Value = doanhThuTongHop[i].SoDonHang;
                    worksheet.Cells[i + 2, 4].Value = doanhThuTongHop[i].SoLuotDangKy; // Thêm số lượt đăng ký dịch vụ

                    worksheet.Cells[i + 2, 2].Style.Numberformat.Format = "#,##0"; // Định dạng tiền tệ
                }

                // Thêm dòng tổng cộng
                var lastRow = doanhThuTongHop.Count + 2;
                worksheet.Cells[lastRow, 1].Value = tongCong.Thang;
                worksheet.Cells[lastRow, 2].Value = tongCong.TongDoanhThu;
                worksheet.Cells[lastRow, 3].Value = tongCong.SoDonHang;
                worksheet.Cells[lastRow, 4].Value = tongCong.SoLuotDangKy; // Tổng số lượt đăng ký dịch vụ

                worksheet.Cells[lastRow, 2].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[lastRow, 1, lastRow, 4].Style.Font.Bold = true;

                // Tự động căn chỉnh cột
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Trả file Excel về client
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"ThongKeDoanhThu_{selectedYear}.xlsx";
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return File(stream, contentType, fileName);
            }
        }

        public IActionResult DoanhThuTheoThang(int? year)
        {
            // Lấy danh sách các năm có trong dữ liệu
            var years = _context.DonHangs
                .Select(dh => dh.NgayTao.Year)
                .Union(_context.PhieuXuats.Select(px => px.NgayXuat.Year))
                .Distinct()
                .OrderBy(y => y)
                .ToList();

            // Nếu không có năm được chọn, mặc định là năm hiện tại
            int selectedYear = year ?? DateTime.Now.Year;
            ViewBag.Years = new SelectList(years, selectedYear);  // Dùng SelectList để binding
            ViewBag.SelectedYear = selectedYear;

            var doanhThuDonHang = _context.DonHangs
                .Where(dh => dh.DaThanhToan && dh.NgayTao.Year == selectedYear)
                .GroupBy(dh => new { dh.NgayTao.Year, dh.NgayTao.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TongTien = g.Sum(dh => dh.TongTien)
                });

            var doanhThuPhieuXuat = _context.PhieuXuats
                .Where(px => px.NgayXuat.Year == selectedYear)
                .GroupBy(px => new { px.NgayXuat.Year, px.NgayXuat.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TongTien = g.Sum(px => px.TongTien)
                });

            var doanhThuTongHop = doanhThuDonHang
                .Concat(doanhThuPhieuXuat)
                .GroupBy(x => new { x.Year, x.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Thang = $"{g.Key.Month}/{g.Key.Year}",
                    TongDoanhThu = g.Sum(x => x.TongTien)
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToList();

            return View(doanhThuTongHop);
        }
    }
}
