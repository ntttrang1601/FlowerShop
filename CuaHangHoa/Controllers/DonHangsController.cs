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
    public class DonHangsController : Controller
    {
        private readonly MyDbContext _context;

        public DonHangsController(MyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 10;  // Số đơn hàng trên mỗi trang

            var query = _context.DonHangs
                .Include(o => o.TrangThaiDH)
                .Include(o => o.CachThanhToan)
                .Include(d => d.User)
                .Include(o => o.DanhGia)
                .OrderByDescending(o => o.NgayTao);

            // Tính tổng số trang
            int totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy dữ liệu của trang hiện tại
            var donHangs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Tạo đối tượng cho ViewModel
            var viewModel = new DonHangListViewModel
            {
                DonHangs = donHangs,
                CurrentPage = page,
                TotalPages = totalPages,
                DanhGias = await _context.DanhGias.ToListAsync()
            };

            return View(viewModel);
        }

        // GET: DonHangs
        //public async Task<IActionResult> Index()
        //{
        //    var myDbContext = _context.DonHangs
        //        .Include(o =>o.TrangThaiDH)
        //        .Include(o =>o.CachThanhToan)
        //        .Include(d => d.User).OrderByDescending(o=>o.NgayTao);
        //    return View(await myDbContext.ToListAsync());
        //}

        // GET: DonHangs/Details/5      
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.User) // Bao gồm thông tin người dùng
                .Include(d => d.ChiTietDHs) // Bao gồm chi tiết đơn hàng
                    .ThenInclude(ct => ct.SanPham)
                    .Include(ct =>ct.DanhGia)// Bao gồm thông tin sản phẩm
                .FirstOrDefaultAsync(m => m.Id == id);

            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }


        // GET: DonHangs/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: DonHangs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TongTien,PhiVanChuyen,UserId,TenCachThanhToan,DaThanhToan,NgayTao,TenTrangThai,DchiGiaoHang,NguoiNhan")] DonHang donHang)
        {
            if (ModelState.IsValid)
            {
                _context.Add(donHang);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", donHang.UserId);
            return View(donHang);
        }

        //// GET: DonHangs/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        //{
        //    if (id == null)
        //    {
        //        return NotFound();
        //    }

        //    var donHang = await _context.DonHangs
        //        .Include( e => e.User)
        //        .Include(e => e.CachThanhToan)
        //        .Include(e => e.ChiTietDHs)
        //            .ThenInclude(e => e.SanPham)
        //                .ThenInclude(e => e.Hinhs)
        //         .SingleOrDefaultAsync(e =>e.Id==id);
        //    if (donHang == null)
        //    {
        //        return NotFound();
        //    }
        //    ViewData["TenCachThanhToan"] = new SelectList(_context.CachThanhToans, "Ten", "Ten", donHang.TenCachThanhToan);
        //    ViewData["TenTrangThai"] = new SelectList(_context.TrangThaiDHs, "Ten", "Ten", donHang.TenTrangThai);
        //    return View(donHang);
        //}

        // GET: DonHangs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(e => e.User) // Lấy thông tin người dùng
                .Include(e => e.CachThanhToan)
                .Include(e => e.ChiTietDHs)
                    .ThenInclude(e => e.SanPham)
                        .ThenInclude(e => e.Hinhs)
                .SingleOrDefaultAsync(e => e.Id == id);

            if (donHang == null)
            {
                return NotFound();
            }

            ViewData["TenCachThanhToan"] = new SelectList(_context.CachThanhToans, "Ten", "Ten", donHang.TenCachThanhToan);
            ViewData["TenTrangThai"] = new SelectList(_context.TrangThaiDHs, "Ten", "Ten", donHang.TenTrangThai);
            ViewBag.UserName = donHang.User?.UserName; // Lưu tên người dùng vào ViewBag

            return View(donHang);
        }


        // POST: DonHangs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TongTien,PhiVanChuyen,UserId,TenCachThanhToan,DaThanhToan,NgayTao,TenTrangThai,DchiGiaoHang,NguoiNhan")] DonHang donHang)
        {
            if (id != donHang.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(donHang);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DonHangExists(donHang.Id))
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

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", donHang.UserId);
            ViewData["TenCachThanhToan"] = new SelectList(_context.CachThanhToans, "Ten", "Ten", donHang.TenCachThanhToan);
            ViewData["TenTrangThai"] = new SelectList(_context.TrangThaiDHs, "Ten", "Ten", donHang.TenTrangThai);
            return View(donHang);
        }

        // GET: DonHangs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donHang = await _context.DonHangs
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (donHang == null)
            {
                return NotFound();
            }

            return View(donHang);
        }

        // POST: DonHangs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var donHang = await _context.DonHangs.FindAsync(id);
            if (donHang != null)
            {
                _context.DonHangs.Remove(donHang);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DonHangExists(int id)
        {
            return _context.DonHangs.Any(e => e.Id == id);
        }
    }
}
