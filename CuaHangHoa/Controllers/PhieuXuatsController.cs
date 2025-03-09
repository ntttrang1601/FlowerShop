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

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class PhieuXuatsController : Controller
    {
        private readonly MyDbContext _context;
        private readonly int _pageSize = 10;
        public PhieuXuatsController(MyDbContext context)
        {
            _context = context;
        }

        // GET: PhieuXuats
        //public async Task<IActionResult> Index()
        //{
        //    //var myDbContext = _context.PhieuXuats.Include(p => p.DangKyDichVu);

        //    var myDbContext = _context.PhieuXuats
        //                      .Include(p => p.DangKyDichVu)
        //                      .ThenInclude(d => d.DichVu) 
        //                      .Include(p => p.DangKyDichVu)
        //                      .ThenInclude(d => d.User)
        //                      .Include(p => p.Staff).OrderByDescending(d=>d.NgayXuat);  

        //    return View(await myDbContext.ToListAsync());

        //}
        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.PhieuXuats
                .Include(p => p.DangKyDichVu)
                .ThenInclude(d => d.DichVu)
                .Include(p => p.DangKyDichVu)
                .ThenInclude(d => d.User)
                .Include(p => p.Staff)
                .OrderByDescending(d => d.NgayXuat);

            // Tổng số phiếu xuất
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Lấy danh sách phiếu xuất cho trang hiện tại
            var phieuXuats = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Tạo view model chứa dữ liệu phân trang
            var viewModel = new PhieuXuatListViewModel
            {
                PhieuXuats = phieuXuats,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: PhieuXuats/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.DangKyDichVu)
                    .ThenInclude(p =>p.DichVu)
                .Include(p => p.DangKyDichVu)
                    .ThenInclude(p => p.User)
                .Include(p => p.CTPhieuXuats)
                    .ThenInclude(p =>p.SanPham)
                .Include(p =>p.Staff)
                .FirstOrDefaultAsync(m => m.Id == id);
           
            if (phieuXuat == null)
            {
                return NotFound();
            }

            return View(phieuXuat);
        }

        // GET: PhieuXuats/Create
        public IActionResult Create()
        {
            ViewData["DangKyDichVuId"] = new SelectList(_context.DangKyDichVus, "Id", "Id");
            return View();
        }

        // POST: PhieuXuats/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DangKyDichVuId,SoLuongMatHang,NgayXuat,PhiPhatSinh,TongTien,GhiChu")] PhieuXuat phieuXuat)
        {
            if (ModelState.IsValid)
            {
                _context.Add(phieuXuat);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["DangKyDichVuId"] = new SelectList(_context.DangKyDichVus, "Id", "Id", phieuXuat.DangKyDichVuId);
            return View(phieuXuat);
        }

        // GET: PhieuXuats/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieuXuat = await _context.PhieuXuats.FindAsync(id);
            if (phieuXuat == null)
            {
                return NotFound();
            }
            ViewData["DangKyDichVuId"] = new SelectList(_context.DangKyDichVus, "Id", "Id", phieuXuat.DangKyDichVuId);
            return View(phieuXuat);
        }

        // POST: PhieuXuats/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DangKyDichVuId,SoLuongMatHang,NgayXuat,PhiPhatSinh,TongTien,GhiChu")] PhieuXuat phieuXuat)
        {
            if (id != phieuXuat.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(phieuXuat);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PhieuXuatExists(phieuXuat.Id))
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
            ViewData["DangKyDichVuId"] = new SelectList(_context.DangKyDichVus, "Id", "Id", phieuXuat.DangKyDichVuId);
            return View(phieuXuat);
        }

        // GET: PhieuXuats/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var phieuXuat = await _context.PhieuXuats
            //    .Include(p => p.DangKyDichVu)
            //    .FirstOrDefaultAsync(m => m.Id == id);

            var phieuXuat = await _context.PhieuXuats
                .Include(p => p.DangKyDichVu)
                    .ThenInclude(p => p.DichVu)
                .Include(p => p.DangKyDichVu)
                    .ThenInclude(p => p.User)
                .Include(p => p.CTPhieuXuats)
                    .ThenInclude(p => p.SanPham)
                .Include(p => p.Staff)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (phieuXuat == null)
            {
                return NotFound();
            }

            return View(phieuXuat);
        }

        // POST: PhieuXuats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var phieuXuat = await _context.PhieuXuats
                .Include(PX=>PX.DangKyDichVu)
        .Include(px => px.CTPhieuXuats)
        .ThenInclude(ct => ct.SanPham)
        .FirstOrDefaultAsync(px => px.Id == id);

            if (phieuXuat == null)
            {
                return NotFound();
            }

            // Cộng lại số lượng sản phẩm
            foreach (var ctPhieuXuat in phieuXuat.CTPhieuXuats)
            {
                var sanPham = ctPhieuXuat.SanPham;
                if (sanPham != null)
                {
                    sanPham.Soluongkho += ctPhieuXuat.SoLuong; // Cộng lại số lượng vào kho
                }
            }
            phieuXuat.DangKyDichVu.TrangThaiDangKy = TrangThaiDK.DaXacNhan;
            // Xóa phiếu xuất
            phieuXuat.IsDeleted = true;
            _context.Update(phieuXuat);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool PhieuXuatExists(int id)
        {
            return _context.PhieuXuats.Any(e => e.Id == id);
        }
    }
}
