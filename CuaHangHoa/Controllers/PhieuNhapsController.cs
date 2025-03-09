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
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using CuaHangHoa.ViewModels;

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class PhieuNhapsController : Controller
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly int _pageSize = 10;

        public PhieuNhapsController(MyDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: PhieuNhaps
        //public async Task<IActionResult> Index()
        //{
        //    var phieuNhaps = await _context.PhieuNhaps
        //.Include(p => p.NhaCungCap)
        //.Include(p => p.User)
        //.OrderByDescending(p=>p.NgayNhap)
        //.ToListAsync();

        //    return View(phieuNhaps);
        //    //return View(await _context.PhieuNhaps.ToListAsync());
        //}

        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.PhieuNhaps
                .Include(p => p.NhaCungCap).Where(p =>p.IsDeleted==false)
                .Include(p => p.User)
                .OrderByDescending(p => p.NgayNhap);

            // Tổng số phiếu nhập
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Lấy danh sách phiếu nhập cho trang hiện tại
            var phieuNhaps = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Tạo view model chứa dữ liệu phân trang
            var viewModel = new PhieuNhapListViewModel
            {
                PhieuNhaps = phieuNhaps,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: PhieuNhaps/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            //if (id == null)
            //{
            //    return NotFound();
            //}

            //var phieuNhap = await _context.PhieuNhaps
            //    .FirstOrDefaultAsync(m => m.Id == id);
            //if (phieuNhap == null)
            //{
            //    return NotFound();
            //}

            //return View(phieuNhap);
            if (id == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.NhaCungCap) 
                .Include(p => p.User)
                .Include(p => p.CTPhieuNhaps)
                    .ThenInclude(ct => ct.SanPham) // Include các sản phẩm của phiếu nhập
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuNhap == null)
            {
                return NotFound();
            }

            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Create
        public IActionResult Create()
        {
            ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps, "Id", "Ten");
            ViewBag.LoaiSPs = new SelectList(_context.LoaiSPs, "Id", "TenLoai"); 
            
            return View();
        }

        //// POST: PhieuNhaps/Create
        //// To protect from overposting attacks, enable the specific properties you want to bind to.
        //// For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        // POST: PhieuNhaps/Create
        // POST: PhieuNhaps/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PhieuNhap phieuNhap)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            phieuNhap.UserId = userId;
            
                // Lấy UserId từ người dùng hiện tại
                
                if (string.IsNullOrEmpty(phieuNhap.GhiChu))
                {
                    phieuNhap.GhiChu = "Không có ghi chú";
                }

                // Nếu cần kiểm tra định dạng TongTien có phải là số nguyên sau khi nhận từ form
                phieuNhap.TongTien = Math.Round(phieuNhap.TongTien, 0);

                var nhaCC = await _context.NhaCungCaps.FindAsync(phieuNhap.NhaCungCapId);
                if (nhaCC != null)
                {
                    phieuNhap.NhaCungCap = nhaCC;
                }

                phieuNhap.SoLuongMatHang = phieuNhap.CTPhieuNhaps.Sum(ct => ct.SoLuong);

                _context.PhieuNhaps.Add(phieuNhap);

                foreach (var ctPhieuNhap in phieuNhap.CTPhieuNhaps)
                {
                    var sanPham = await _context.SanPhams.FindAsync(ctPhieuNhap.SanPhamId);
                    if (sanPham != null)
                    {
                        sanPham.Soluongkho += ctPhieuNhap.SoLuong;
                        _context.SanPhams.Update(sanPham);
                    }
                }
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi lưu dữ liệu: " + ex.Message);
                    // Đưa lại ViewBag để hiển thị lại form
                    ViewBag.NhaCungCaps = new SelectList(_context.NhaCungCaps.ToList(), "Id", "Ten", phieuNhap.NhaCungCapId);
                    ViewBag.LoaiSPs = new SelectList(_context.LoaiSPs.ToList(), "Id", "TenLoai");
                    return View(phieuNhap);
                }
                
          

            
        }








        // API lấy sản phẩm theo loại sản phẩm
        [HttpGet]
        [HttpGet]
        public IActionResult GetProductsByCategory(int categoryId)
        {
            var products = _context.SanPhams
                .Where(sp => sp.LoaiSPId == categoryId && sp.Ngungban == false)
                .Select(sp => new { id = sp.Id, ten = sp.Ten })
                .ToList();

            return Json(products);
        }


        // GET: PhieuNhaps/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhaps.FindAsync(id);
            if (phieuNhap == null)
            {
                return NotFound();
            }
            return View(phieuNhap);
        }

        // POST: PhieuNhaps/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,NhaCCId,NgayNhap,SoLuongMatHang,TongTien,GhiChu")] PhieuNhap phieuNhap)
        {
            if (id != phieuNhap.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuNhap);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuNhapExists(phieuNhap.Id))
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
            return View(phieuNhap);
        }

        // GET: PhieuNhaps/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.NhaCungCap)
                .Include(p => p.User)
                .Include(p => p.CTPhieuNhaps)
                    .ThenInclude(ct => ct.SanPham) // Include các sản phẩm của phiếu nhập
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuNhap == null)
            {
                return NotFound();
            }

            return View(phieuNhap);
        }


        // POST: PhieuNhaps/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Tìm phiếu nhập và bao gồm danh sách sản phẩm chi tiết
            var phieuNhap = await _context.PhieuNhaps
                .Include(p => p.CTPhieuNhaps)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuNhap != null)
            {
                // Giảm số lượng sản phẩm trong kho theo số lượng nhập trong phiếu nhập này
                foreach (var chiTiet in phieuNhap.CTPhieuNhaps)
                {
                    var sanPham = await _context.SanPhams.FindAsync(chiTiet.SanPhamId);
                    if (sanPham != null)
                    {
                        sanPham.Soluongkho -= chiTiet.SoLuong; // Trừ số lượng sản phẩm đã nhập
                    }
                }

                // Xóa phiếu nhập
                phieuNhap.IsDeleted = true;
                _context.Update(phieuNhap);

                // Lưu các thay đổi
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }



        //// GET: PhieuNhaps/Delete/5
        //public async Task<IActionResult> Delete(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var phieuNhap = await _context.PhieuNhaps
        //        .FirstOrDefaultAsync(m => m.Id == id);
        //    if (phieuNhap == null)
        //    {
        //        return NotFound();
        //    }

        //    return View(phieuNhap);
        //}

        //// POST: PhieuNhaps/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(int id)
        //{
        //    var phieuNhap = await _context.PhieuNhaps.FindAsync(id);
        //    if (phieuNhap != null)
        //    {
        //        _context.PhieuNhaps.Remove(phieuNhap);
        //    }

        //    await _context.SaveChangesAsync();
        //    return RedirectToAction(nameof(Index));
        //}

        private bool PhieuNhapExists(int id)
        {
            return _context.PhieuNhaps.Any(e => e.Id == id);
        }
    }
}
