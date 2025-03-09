using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CuaHangHoa.Data;
using CuaHangHoa.Models;
using Microsoft.AspNetCore.Authorization;
using CuaHangHoa.ViewModels;
using System.Security.Cryptography;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class DangKyDichVusController : Controller
    {
        private readonly MyDbContext _context;
        
        
        public DangKyDichVusController(MyDbContext context)
        {
            _context = context;
            
            
        }

        // GET: DangKyDichVus

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10; 
            var query = _context.DangKyDichVus
                .Include(d => d.DichVu)
                .Include(d => d.User)
                .OrderByDescending(d => d.NgayDangKy);

            // Tính tổng số đơn vị dữ liệu
            int totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy dữ liệu của trang hiện tại
            var dangKyDichVus = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Truyền dữ liệu sang View
            var viewModel = new DangKyDichVuListViewModel
            {
                DangKyDichVus = dangKyDichVus,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        //public async Task<IActionResult> Index()
        //{
        //    var myDbContext = _context.DangKyDichVus
        //        .Include(d => d.DichVu)
        //        .Include(d => d.User).OrderByDescending(d => d.NgayDangKy);
        //    return View(await myDbContext.ToListAsync());
        //}

        // GET: DangKyDichVus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dangKyDichVu = await _context.DangKyDichVus
                .Include(d => d.DichVu)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dangKyDichVu == null)
            {
                return NotFound();
            }

            return View(dangKyDichVu);
        }

        // GET: DangKyDichVus/Create
        public IActionResult Create()
        {
            ViewData["DichVuId"] = new SelectList(_context.DichVus, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: DangKyDichVus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DichVuId,UserId,NgayDangKy,NgayToChuc,TrangThaiDangKy,DchiToChuc,GhiChu")] DangKyDichVu dangKyDichVu)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dangKyDichVu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DichVuId"] = new SelectList(_context.DichVus, "Id", "Id", dangKyDichVu.DichVuId);
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dangKyDichVu.UserId);
            return View(dangKyDichVu);
        }

        // GET: DangKyDichVus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var dangKyDichVu = await _context.DangKyDichVus           
            .Include(d => d.DichVu) // Include dịch vụ
            .ThenInclude(d => d.HinhDVs) // Include hình ảnh của dịch vụ
            .FirstOrDefaultAsync(m => m.Id == id);
            //var dangKyDichVu = await _context.DangKyDichVus.FindAsync(id);
            if (dangKyDichVu == null)
            {
                return NotFound();
            }

            // Lấy tên dịch vụ và tên người dùng
            ViewBag.DichVuName = dangKyDichVu.DichVu?.TenDV; // Tên dịch vụ
            ViewBag.UserName = await _context.Users
                .Where(u => u.Id == dangKyDichVu.UserId)
                .Select(u => u.UserName) // Lấy tên người dùng
                .FirstOrDefaultAsync();

            ViewBag.TrangThaiDangKyList = Enum.GetValues(typeof(TrangThaiDK))
                .Cast<TrangThaiDK>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString(),
                    Selected = (e == dangKyDichVu.TrangThaiDangKy)
                }).ToList();



            //ViewData["DichVuId"] = new SelectList(_context.DichVus, "Id", "Id", dangKyDichVu.DichVuId);
            //ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dangKyDichVu.UserId);
            return View(dangKyDichVu);
        }

        // POST: DangKyDichVus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DichVuId,UserId,NgayDangKy,NgayToChuc,TrangThaiDangKy,DchiToChuc,GhiChu")] DangKyDichVu dangKyDichVu)
        {
            if (id != dangKyDichVu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(dangKyDichVu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DangKyDichVuExists(dangKyDichVu.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewBag.TrangThaiDangKyList = Enum.GetValues(typeof(TrangThaiDK))
                .Cast<TrangThaiDK>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString(),
                    Selected = (e == dangKyDichVu.TrangThaiDangKy)
                }).ToList();
            //ViewData["DichVuId"] = new SelectList(_context.DichVus, "Id", "Id", dangKyDichVu.DichVuId);
            //ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", dangKyDichVu.UserId);
            return View(dangKyDichVu);
        }

        // GET: DangKyDichVus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dangKyDichVu = await _context.DangKyDichVus
                .Include(d => d.DichVu)
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dangKyDichVu == null)
            {
                return NotFound();
            }

            return View(dangKyDichVu);
        }

        // POST: DangKyDichVus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dangKyDichVu = await _context.DangKyDichVus.FindAsync(id);
            if (dangKyDichVu != null)
            {
                _context.DangKyDichVus.Remove(dangKyDichVu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
      
        private bool DangKyDichVuExists(int id)
        {
            return _context.DangKyDichVus.Any(e => e.Id == id);
        }

        [HttpGet]
        public IActionResult CreatePhieuXuat(int? id)
        {
            if (id == null) return NotFound();

            var dangKyDichVu = _context.DangKyDichVus
                .Include(d => d.DichVu)
                .FirstOrDefault(d => d.Id == id);

            if (dangKyDichVu == null) return NotFound();
            // Lấy tên người đặt từ UserId
            var user = _context.Users.FirstOrDefault(u => u.Id == dangKyDichVu.UserId);
            ViewBag.UserName = user?.UserName; // Nếu có UserName trong bảng Users

            ViewBag.DangKyDichVuId = dangKyDichVu.Id; // Gửi ID DangKyDichVu sang view
            ViewBag.DichVuTen = dangKyDichVu.DichVu.TenDV;
            ViewBag.LoaiSPs = new SelectList(_context.LoaiSPs, "Id", "TenLoai");

            return View();
        }

        [HttpPost]
        public IActionResult CreatePhieuXuat(PhieuXuat phieuXuat, List<CTPhieuXuat> ctPhieuXuats)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            phieuXuat.StaffId = userId;
            
                
                // Xóa giá trị Id để EF tự động sinh Id mớij
                phieuXuat.Id = 0;

                // Tính tổng tiền phiếu xuất dựa trên chi tiết và phí phát sinh
                double tongTienChiTiet = ctPhieuXuats.Sum(ct => ct.SoLuong * ct.GiaSP);
                phieuXuat.TongTien = tongTienChiTiet + phieuXuat.PhiPhatSinh;

                // Tính tổng số lượng mặt hàng
                phieuXuat.SoLuongMatHang = ctPhieuXuats.Sum(ct => ct.SoLuong);

                // Lưu PhieuXuat vào cơ sở dữ liệu trước để lấy Id của nó
                _context.PhieuXuats.Add(phieuXuat);
                _context.SaveChanges();  // Save once to get PhieuXuat Id

                // Kiểm tra nếu các chi tiết đã được gán ID phiếu xuất
                foreach (var ct in ctPhieuXuats)
                {

                    // Đảm bảo rằng chi tiết phiếu xuất chưa được lưu trước đó
                    var existingDetail = _context.CTPhieuXuats
                .FirstOrDefault(d => d.PhieuXuatId == phieuXuat.Id && d.SanPhamId == ct.SanPhamId);

                    if (existingDetail == null)
                    {
                        ct.PhieuXuatId = phieuXuat.Id;  // Gán Id của phiếu xuất vào chi tiết
                        _context.CTPhieuXuats.Add(ct); // Thêm vào context
                    }

                    // Lấy sản phẩm từ cơ sở dữ liệu
                    var sanPham = _context.SanPhams.FirstOrDefault(sp => sp.Id == ct.SanPhamId);
                    if (sanPham != null)
                    {
                        // Kiểm tra số lượng kho còn đủ hay không
                        if (sanPham.Soluongkho < ct.SoLuong)
                        {
                            ModelState.AddModelError("", $"Sản phẩm '{sanPham.Ten}' không đủ số lượng trong kho.");
                            SetViewBagData(phieuXuat.DangKyDichVuId);
                            return View(phieuXuat);
                        }

                        // Trừ số lượng kho của sản phẩm
                        sanPham.Soluongkho -= ct.SoLuong;
                        _context.SanPhams.Update(sanPham);
                    }
                }
                try
                {
                    // Lưu chi tiết phiếu xuất và cập nhật kho
                    _context.SaveChanges();
                    // Cập nhật trạng thái của DangKyDichVu thành "Đã hoàn thành"
                    var dangKyDichVu = _context.DangKyDichVus.Find(phieuXuat.DangKyDichVuId);
                    if (dangKyDichVu != null)
                    {
                        dangKyDichVu.TrangThaiDangKy = TrangThaiDK.DaHoanThanh;
                        _context.SaveChanges();
                    }

                    return RedirectToAction("Index", "PhieuXuats"); // Chuyển hướng sau khi lưu thành công
                }
                catch (Exception ex)
                {
                    // Nếu ModelState không hợp lệ, lấy lại thông tin cho view
                    SetViewBagData(phieuXuat.DangKyDichVuId);

                    // Trả lại view để người dùng sửa thông tin
                    return View(phieuXuat);
                }

                
            

            
            
        }



        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult CreatePhieuXuat(PhieuXuat phieuXuat, List<CTPhieuXuat> ctPhieuXuats)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        if (string.IsNullOrEmpty(phieuXuat.GhiChu))
        //        {
        //            phieuXuat.GhiChu = "Không có ghi chú";
        //        }
        //        // Xóa giá trị Id để EF tự động sinh Id mới
        //        phieuXuat.Id = 0;

        //        // Tính tổng tiền phiếu xuất dựa trên chi tiết và phí phát sinh
        //        double tongTienChiTiet = ctPhieuXuats.Sum(ct => ct.SoLuong * ct.GiaSP);
        //        phieuXuat.TongTien = tongTienChiTiet + phieuXuat.PhiPhatSinh;

        //        // Tính tổng số lượng mặt hàng
        //        phieuXuat.SoLuongMatHang = ctPhieuXuats.Sum(ct => ct.SoLuong);

        //        // Lưu PhieuXuat vào cơ sở dữ liệu để lấy Id của nó
        //        _context.PhieuXuats.Add(phieuXuat);
        //        _context.SaveChanges();

        //        // Cập nhật số lượng kho của sản phẩm và lưu CTPhieuXuat
        //        foreach (var ct in ctPhieuXuats)
        //        {
        //            // Lấy sản phẩm từ cơ sở dữ liệu
        //            var sanPham = _context.SanPhams.Find(ct.SanPhamId);
        //            if (sanPham != null)
        //            {
        //                // Trừ số lượng kho
        //                sanPham.Soluongkho -= ct.SoLuong;

        //                // Kiểm tra nếu số lượng kho < 0, trả về lỗi (tùy chọn)
        //                if (sanPham.Soluongkho < 0)
        //                {
        //                    ModelState.AddModelError("", $"Số lượng kho của sản phẩm '{sanPham.Ten}' không đủ.");
        //                    SetViewBagData(phieuXuat.DangKyDichVuId);
        //                    return View(phieuXuat);
        //                }

        //                // Cập nhật CTPhieuXuat với Id của phiếu xuất
        //                ct.PhieuXuatId = phieuXuat.Id;
        //                _context.CTPhieuXuats.Add(ct);
        //            }
        //        }

        //        // Lưu các chi tiết vào cơ sở dữ liệu và cập nhật số lượng kho
        //        _context.SaveChanges();

        //        // Cập nhật trạng thái của DangKyDichVu thành "Đã hoàn thành"
        //        var dangKyDichVu = _context.DangKyDichVus.Find(phieuXuat.DangKyDichVuId);
        //        if (dangKyDichVu != null)
        //        {
        //            dangKyDichVu.TrangThaiDangKy = TrangThaiDK.DaHoanThanh;
        //            _context.SaveChanges();
        //        }

        //        return RedirectToAction("Index"); // Chuyển hướng sau khi lưu thành công
        //    }

        //    // Nếu ModelState không hợp lệ, lấy lại thông tin cho view
        //    SetViewBagData(phieuXuat.DangKyDichVuId);

        //    // Trả lại view để người dùng sửa thông tin
        //    return View(phieuXuat);
        //}

        private void SetViewBagData(int dkdichvuId)
        {
            var dangKyDichVu = _context.DangKyDichVus
                .Include(d => d.DichVu)
                .Include(d => d.User)
                .FirstOrDefault(d => d.Id == dkdichvuId);

            ViewBag.DichVuTen = dangKyDichVu?.DichVu?.TenDV ?? "Không có thông tin dịch vụ";
            ViewBag.UserName = dangKyDichVu?.User?.UserName ?? "Không tìm thấy thông tin người dùng";
            ViewBag.LoaiSPs = new SelectList(_context.LoaiSPs, "Id", "TenLoai");
        }



        [HttpGet]
        public IActionResult GetProductsByCategory(int categoryId)
        {
            var products = _context.SanPhams
        .Where(sp => sp.LoaiSPId == categoryId && sp.Ngungban == false)
        .Select(sp => new
        {
            id = sp.Id,
            ten = sp.Ten,
            gia = sp.Dongia // Thêm trường giá
        })
        .ToList();

            // Kiểm tra dữ liệu để đảm bảo giá đã được lấy
            // Bạn có thể ghi log hoặc debug ở đây
            return Json(products);
        }

    }
}
