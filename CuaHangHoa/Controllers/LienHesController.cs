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
    public class LienHesController : Controller
    {
        private readonly MyDbContext _context;
        private readonly int _pageSize = 10;

        public LienHesController(MyDbContext context)
        {
            _context = context;
        }

        // GET: LienHes
        //public async Task<IActionResult> Index()
        //{
        //    return View(await _context.LienHes.OrderByDescending(s=>s.Id).ToListAsync());
        //}
        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.LienHes.OrderByDescending(s => s.Id);  // Sắp xếp theo Id giảm dần

            var totalItems = await query.CountAsync();  // Đếm tổng số bản ghi
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);  // Tính tổng số trang

            var items = await query
                .Skip((page - 1) * _pageSize)  // Bỏ qua các mục trước trang hiện tại
                .Take(_pageSize)  // Lấy số mục của trang hiện tại
                .ToListAsync();

            // Tạo đối tượng viewmodel để trả về cho view
            var viewModel = new LienHeListViewModel
            {
                LienHes = items,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        // GET: LienHes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lienHe = await _context.LienHes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lienHe == null)
            {
                return NotFound();
            }

            return View(lienHe);
        }

        // GET: LienHes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: LienHes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,HoTen,SDT,MatHang,ThongDiep,NgayGui,TTLienHe,GhiChu")] LienHe lienHe)
        {
            if (ModelState.IsValid)
            {
                _context.Add(lienHe);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(lienHe);
        }

        // GET: LienHes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lienHe = await _context.LienHes.FindAsync(id);
            if (lienHe == null)
            {
                return NotFound();
            }
            ViewBag.TTLienHeList = Enum.GetValues(typeof(TrangThaiLH))
                .Cast<TrangThaiLH>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString(),
                    Selected = (e == lienHe.TTLienHe)
                }).ToList();
            return View(lienHe);
        }

        // POST: LienHes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,HoTen,SDT,MatHang,ThongDiep,NgayGui,TTLienHe,GhiChu,TenNhanVien")] LienHe lienHe)
        {
            if (id != lienHe.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    lienHe.TenNhanVien = User.Identity.Name;
                    _context.Update(lienHe);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LienHeExists(lienHe.Id))
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
            ViewBag.TTLienHeList = Enum.GetValues(typeof(TrangThaiLH))
                .Cast<TrangThaiLH>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString(),
                    Selected = (e == lienHe.TTLienHe)
                }).ToList();
            return View(lienHe);
        }

        // GET: LienHes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lienHe = await _context.LienHes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (lienHe == null)
            {
                return NotFound();
            }

            return View(lienHe);
        }

        // POST: LienHes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lienHe = await _context.LienHes.FindAsync(id);
            if (lienHe != null)
            {
                _context.LienHes.Remove(lienHe);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool LienHeExists(int id)
        {
            return _context.LienHes.Any(e => e.Id == id);
        }
    }
}
