using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CuaHangHoa.Data;
using CuaHangHoa.Models;
using System.Data;
using NuGet.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using CuaHangHoa.ViewModels;

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin")]
    public class SanPhamsController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly int _pageSize = 5;
        public SanPhamsController(MyDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: SanPhams
        //public async Task<IActionResult> Index()
        //{
        //    var myDbContext = _context.SanPhams.Include(s => s.LoaiSP)
        //        //.Include(s => s.NhaCungCap)
        //        .Include(s => s.Hinhs).OrderByDescending(s=>s.Id);
        //    return View(await myDbContext.ToListAsync());
        //}

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            var query = _context.SanPhams.Include(s => s.LoaiSP)
                .Include(s => s.Hinhs) // Nếu bạn cần hiển thị ảnh sản phẩm
                .OrderByDescending(s => s.Id);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(s => s.Ten.Contains(searchQuery)).OrderByDescending(s => s.Id);
            }

            // Tổng số sản phẩm
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Lấy danh sách sản phẩm cho trang hiện tại
            var products = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Tạo view model chứa dữ liệu phân trang
            var viewModel = new ProductListViewModel
            {
                SanPhams = products,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchQuery = searchQuery
            };

            return View(viewModel);
        }

        // GET: SanPhams/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.LoaiSP)
                //.Include(s => s.NhaCungCap)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // GET: SanPhams/Create
        public IActionResult Create()
        {
            ViewData["LoaiSPId"] = new SelectList(_context.LoaiSPs, "Id", "TenLoai");
            ViewData["NhaCungCapId"] = new SelectList(_context.NhaCungCaps, "Id", "Ten");
            return View();
        }

        // POST: SanPhams/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Ten,Mota,Donvitinh,Dongia,Soluongkho,Ngungban,PhanTramGiamGia,GiaSauGiamGia,LoaiSPId")] SanPham sanPham,
            [Bind("files")] List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                sanPham.Soluongkho = 0;
                sanPham.Ngungban = false;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        var folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/SanPham");
                        var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(folder, fileName);

                        var image = new Hinh
                        {
                            SanPham = sanPham,
                            SanPhamId = sanPham.Id,
                            Url = fileName,
                        };
                        sanPham.Hinhs.Add(image);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
                _context.Add(sanPham);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["LoaiSPId"] = new SelectList(_context.LoaiSPs, "Id", "Id", sanPham.LoaiSPId);
            //ViewData["NhaCungCapId"] = new SelectList(_context.NhaCungCaps, "Id", "Id", sanPham.NhaCungCapId);
            return View(sanPham);
        }

        // GET: SanPhams/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(e => e.Hinhs)
                .SingleAsync(e => e.Id == id);
            if (sanPham == null)
            {
                return NotFound();
            }
            ViewData["LoaiSPId"] = new SelectList(_context.LoaiSPs, "Id", "TenLoai", sanPham.LoaiSPId);
            //ViewData["NhaCungCapId"] = new SelectList(_context.NhaCungCaps, "Id", "Ten", sanPham.NhaCungCapId);
            return View(sanPham);
        }

        // POST: SanPhams/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Ten,Mota,Donvitinh,Dongia,Soluongkho,Ngungban,PhanTramGiamGia,GiaSauGiamGia,LoaiSPId")] SanPham sanPham,
            [Bind("files")] List<IFormFile> files)
        {
            if (id != sanPham.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (files != null && files.Count > 0)
                    {
                        var lstImage = _context.Hinhs.Where(x => x.SanPhamId == sanPham.Id);
                        if (lstImage.Any())
                        {
                            _context.Hinhs.RemoveRange(lstImage);
                            foreach (var image in lstImage)
                            {
                                var path = Path.Combine(_webHostEnvironment.WebRootPath, "images/SanPham");
                                var pathFile = Path.Combine(path, image.Url);
                                if (System.IO.File.Exists(pathFile))
                                {
                                    System.IO.File.Delete(pathFile);
                                }
                            }
                        }
                        foreach (var file in files)
                        {
                            var folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/SanPham");
                            var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            var filePath = Path.Combine(folder, fileName);

                            var image = new Hinh
                            {
                                SanPham = sanPham,
                                SanPhamId = sanPham.Id,
                                Url = fileName,
                            };
                            sanPham.Hinhs.Add(image);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                        }
                    }
                    _context.Update(sanPham);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(sanPham.Id))
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
            ViewData["LoaiSPId"] = new SelectList(_context.LoaiSPs, "Id", "Id", sanPham.LoaiSPId);
            //ViewData["NhaCungCapId"] = new SelectList(_context.NhaCungCaps, "Id", "Id", sanPham.NhaCungCapId);
            return View(sanPham);
        }

        // GET: SanPhams/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = await _context.SanPhams
                .Include(s => s.LoaiSP)
                //.Include(s => s.NhaCungCap)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        // POST: SanPhams/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sanPham = await _context.SanPhams.FindAsync(id);
            if (sanPham == null)
            {
                return NotFound();
            }
            sanPham.Ngungban = true;
            _context.Update(sanPham);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SanPhamExists(int id)
        {
            return _context.SanPhams.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> SearchProducts(string searchQuery)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return Json(new List<object>()); // Trả về danh sách trống nếu không có từ khóa
            }

            var products = await _context.SanPhams
                .Where(s => s.Ten.Contains(searchQuery))
                .Select(s => new
                {
                    id = s.Id,
                    ten = s.Ten
                })
                .Take(10) // Giới hạn số sản phẩm trả về
                .ToListAsync();

            return Json(products);
        }
    }
}
