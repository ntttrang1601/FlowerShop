using CuaHangHoa.Data;
using CuaHangHoa.ViewModels;
using CuaHangHoa.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CuaHangHoa.Controllers
{
    public class ServiceController : Controller
    {
        private readonly MyDbContext _dbContext;
        public ServiceController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            var viewModel = new DVuViewModel
            {
                DichVus = _dbContext.DichVus
            .Where(dv => dv.Ngungban == false)  // Thêm điều kiện Ngungban == 0
            .ToList(),

                HinhCuaDVs = _dbContext.HinhDVs
            .Where(img => img.DichVu.Ngungban == false)  // Thêm điều kiện Ngungban == 0 cho hình ảnh dịch vụ
            .GroupBy(img => img.DichVu.Id)
            .Select(group => group.FirstOrDefault())
            .ToList()
            };
            return View(viewModel);

        }

        [HttpGet]
        public async Task<IActionResult> ServiceDetail(int id)
        {
            var sv = await _dbContext.DichVus
                .Include(e => e.HinhDVs)                
                .SingleOrDefaultAsync(e => e.Id == id);
            if (sv == null)
            {
                return NotFound();
            }
            else
            {
                return View(sv);
            }
        }
        public IActionResult RegisterService(int id)
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var dichVu = _dbContext.DichVus
                .Include(d => d.HinhDVs) // Include để lấy hình ảnh của dịch vụ
                .FirstOrDefault(d => d.Id == id);

            if (dichVu == null)
            {
                return NotFound();
            }

            // Lấy hình ảnh đầu tiên của dịch vụ
            var firstImage = dichVu.HinhDVs.FirstOrDefault()?.Url ?? "default-image.png"; // Nếu không có hình thì dùng ảnh mặc định

            var viewModel = new DKDichVuViewModel
            {
                DichVuId = dichVu.Id,
                TenDV = dichVu.TenDV,
                NgayToChuc = DateTime.Now,
                DchiToChuc = "", // Có thể để trống
                Images = firstImage // Gán ảnh đầu tiên vào ViewModel
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RegisterService(DKDichVuViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Log tất cả các lỗi trong ModelState
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                foreach (var error in errors)
                {
                    Console.WriteLine(error); // Log ra lỗi
                }
                return View(viewModel); // Trả lại view với thông tin lỗi
            }
            try
            {
                var user = User.FindFirst(ClaimTypes.NameIdentifier);
                var dangKyDichVu = new DangKyDichVu
                {
                    DichVuId = viewModel.DichVuId,
                    UserId = user.Value, // Lấy UserId
                    NgayDangKy = DateTime.Now,
                    NgayToChuc = viewModel.NgayToChuc,
                    DchiToChuc = viewModel.DchiToChuc,
                    GhiChu = viewModel.GhiChu,
                    TrangThaiDangKy = TrangThaiDK.DangXuLy
                };

                _dbContext.DangKyDichVus.Add(dangKyDichVu);
                _dbContext.SaveChanges();

                return RedirectToAction("RServiceDetail", new { id = dangKyDichVu.Id });
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                Console.WriteLine("Error: " + ex.ToString()); // In ra chi tiết exception
                return View(viewModel); // Trả lại view nếu có lỗi
            }
        }

        //public IActionResult RegisterService(int id)
        //{
        //    var user = User.FindFirst(ClaimTypes.NameIdentifier);
        //    if (user == null)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }
        //    var dichVu = _dbContext.DichVus.FirstOrDefault(d => d.Id == id);

        //    if (dichVu == null)
        //    {
        //        return NotFound();
        //    }

        //    var viewModel = new DKDichVuViewModel
        //    {
        //        DichVuId = dichVu.Id,
        //        TenDV=dichVu.TenDV,
        //        NgayToChuc=DateTime.Now,
        //        DchiToChuc = "" // có thể để trống
        //    };

        //    return View(viewModel);
        //}

        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult RegisterService(DKDichVuViewModel viewModel)
        //{            
        //    if (!ModelState.IsValid)
        //    {
        //        // Log tất cả các lỗi trong ModelState
        //        var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
        //        foreach (var error in errors)
        //        {
        //            Console.WriteLine(error); // Log ra lỗi
        //        }
        //        return View(viewModel); // Trả lại view với thông tin lỗi
        //    }
        //    try
        //    {
        //        var user = User.FindFirst(ClaimTypes.NameIdentifier);
        //        var dangKyDichVu = new DangKyDichVu
        //        {
        //            DichVuId = viewModel.DichVuId,
        //            UserId = user.Value, // Lấy UserId
        //            NgayDangKy = DateTime.Now,
        //            NgayToChuc = viewModel.NgayToChuc,
        //            DchiToChuc = viewModel.DchiToChuc,
        //            GhiChu = viewModel.GhiChu,
        //            TrangThaiDangKy = TrangThaiDK.DangXuLy
        //        };                
        //        _dbContext.DangKyDichVus.Add(dangKyDichVu);
        //        _dbContext.SaveChanges();

        //        return RedirectToAction("RServiceDetail", new { id = dangKyDichVu.Id });
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log chi tiết lỗi
        //        Console.WriteLine("Error: " + ex.ToString()); // In ra chi tiết exception
        //        return View(viewModel); // Trả lại view nếu có lỗi
        //    }



        //}

        [HttpGet]
        public IActionResult RServiceList()
        {
            // Lấy UserId của người dùng hiện tại
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Nếu người dùng chưa đăng nhập, chuyển về trang login
            }

            // Truy vấn cơ sở dữ liệu để lấy danh sách dịch vụ đã đăng ký của người dùng
            var danhSachDangKy = _dbContext.DangKyDichVus
                .Include(dk => dk.DichVu) // Include để lấy thông tin dịch vụ
                .ThenInclude(dv => dv.HinhDVs) // Include để lấy danh sách hình ảnh của dịch vụ
                .Where(dk => dk.UserId == userId)
                .ToList();

            // Truyền danh sách vào ViewModel và gán hình ảnh
            var viewModel = danhSachDangKy.Select(dk => new DKDichVuViewModel
            {
                Id = dk.Id,
                DichVuId = dk.DichVuId,
                TenDV = dk.DichVu.TenDV,
                NgayDangKy = dk.NgayDangKy,
                NgayToChuc = dk.NgayToChuc,
                TrangThaiDangKy = dk.TrangThaiDangKy,
                // Lấy hình ảnh đầu tiên của dịch vụ, nếu không có thì gán ảnh mặc định
                Images = dk.DichVu.HinhDVs.FirstOrDefault()?.Url ?? "default-image.png"
            }).ToList();

            // Trả về view và truyền viewModel để hiển thị danh sách
            return View(viewModel);
        }


        public IActionResult RServiceDetail(int id)
        {
            // Lấy thông tin đăng ký dịch vụ bằng Id, bao gồm cả dịch vụ và hình ảnh
            var dangKyDichVu = _dbContext.DangKyDichVus
                .Include(d => d.DichVu)
                .ThenInclude(dv => dv.HinhDVs) // Include để lấy hình ảnh của dịch vụ
                .FirstOrDefault(d => d.Id == id);

            if (dangKyDichVu == null)
            {
                return NotFound();
            }

            // Lấy hình ảnh đầu tiên của dịch vụ
            var firstImage = dangKyDichVu.DichVu.HinhDVs.FirstOrDefault()?.Url ?? "default-image.png"; // Nếu không có hình, dùng ảnh mặc định

            var viewModel = new DKDichVuViewModel
            {
                Id = dangKyDichVu.Id,
                DichVuId = dangKyDichVu.DichVuId,
                TenDV = dangKyDichVu.DichVu.TenDV,
                NgayDangKy = dangKyDichVu.NgayDangKy,
                NgayToChuc = dangKyDichVu.NgayToChuc,
                DchiToChuc = dangKyDichVu.DchiToChuc,
                GhiChu = dangKyDichVu.GhiChu,
                TrangThaiDangKy = dangKyDichVu.TrangThaiDangKy,
                Images = firstImage // Gán hình ảnh vào ViewModel
            };

            return View(viewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HuyDangKy(int dangKyId)
        {
            // Lấy UserId của người dùng hiện tại
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                return RedirectToAction("Login", "Account"); // Nếu người dùng chưa đăng nhập, chuyển về trang login
            }

            // Tìm đăng ký dịch vụ cần hủy
            var dangKyDichVu = _dbContext.DangKyDichVus
                .FirstOrDefault(dk => dk.Id == dangKyId && dk.UserId == userId);

            if (dangKyDichVu == null)
            {
                return NotFound(); // Nếu không tìm thấy đăng ký nào khớp
            }

            // Cập nhật trạng thái thành "Đã Hủy"
            dangKyDichVu.TrangThaiDangKy = TrangThaiDK.DaHuy;

            // Lưu thay đổi vào cơ sở dữ liệu
            _dbContext.SaveChanges();

            // Quay lại trang chi tiết hoặc danh sách đăng ký
            return RedirectToAction("RServiceDetail", new { id = dangKyId });
        }

        [HttpGet]
        public IActionResult PXuatDetail(int id)
        {
            // Truy vấn thông tin chi tiết phiếu xuất theo Id của Đăng ký dịch vụ
            var phieuXuat = _dbContext.PhieuXuats
                .Include(px => px.DangKyDichVu)               // Lấy thông tin đăng ký dịch vụ
                    .ThenInclude(d => d.DichVu)              // Lấy thông tin dịch vụ
                .Include(px => px.DangKyDichVu)               // Lấy thông tin người đặt
                    .ThenInclude(d => d.User)
                .Include(px => px.CTPhieuXuats)               // Lấy chi tiết phiếu xuất
                    .ThenInclude(ct => ct.SanPham)           // Lấy thông tin sản phẩm
                .Include(px => px.Staff)                      // Lấy thông tin người xuất (staff)
                .FirstOrDefault(px => px.DangKyDichVuId == id);


            if (phieuXuat == null)
            {
                return NotFound();
            }

            // Truyền dữ liệu sang view
            return View(phieuXuat);
        }



    }
}
