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
using CuaHangHoa.Helpers;
using CuaHangHoa.ViewModels;

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DichVusController : Controller
    {
        private readonly MyDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public DichVusController(MyDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: DichVus
        //public async Task<IActionResult> Index()
        //{
        //    var myDbContext = _context.DichVus.Include(s => s.HinhDVs).OrderByDescending(s=>s.Id);
        //    return View(await myDbContext.ToListAsync());
        //    //return View(await _context.DichVus.ToListAsync());
        //}

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            int pageSize = 5; // Mỗi trang sẽ hiển thị 10 dòng
            var query = _context.DichVus.Include(s => s.HinhDVs).OrderByDescending(s => s.Id);
            // Áp dụng tìm kiếm nếu có từ khóa
            //if (!string.IsNullOrEmpty(searchQuery))
            //{
            //    query = query.Where(s => s.TenDV.Contains(searchQuery));
            //}
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(s => s.TenDV.Contains(searchQuery)).OrderByDescending(s => s.Id);
            }

            // Tính tổng số dịch vụ
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy dữ liệu dịch vụ của trang hiện tại
            var services = await query
                .Skip((page - 1) * pageSize) // Bỏ qua các dịch vụ ở các trang trước
                .Take(pageSize) // Lấy 10 dịch vụ cho trang hiện tại
                .ToListAsync();


            // Truyền dữ liệu sang View
            var viewModel = new DichVuListViewModel
            {
                DichVus = services,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchQuery = searchQuery // Truyền từ khóa tìm kiếm
            };

            return View(viewModel);
        }


        // GET: DichVus/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dichVu = await _context.DichVus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dichVu == null)
            {
                return NotFound();
            }

            return View(dichVu);
        }

        // GET: DichVus/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: DichVus/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TenDV,Mota,Gia,Ngungban")] DichVu dichVu,[Bind("files")] List<IFormFile> files)
        {
            if (ModelState.IsValid)
            {
                dichVu.Ngungban = false;
                if (files != null && files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        var folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/DichVu");
                        var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                        var filePath = Path.Combine(folder, fileName);

                        var image = new HinhDV
                        {
                            DichVu = dichVu,
                            DichVuId = dichVu.Id,
                            Url = fileName,
                        };
                        dichVu.HinhDVs.Add(image);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                    }
                }
                _context.Add(dichVu);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(dichVu);
        }

        // GET: DichVus/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //var dichVu = await _context.DichVus.FindAsync(id);
            //if (dichVu == null)
            //{
            //    return NotFound();
            //}
            var dichVu = await _context.DichVus
                .Include(e => e.HinhDVs)
                .SingleAsync(e => e.Id == id);
            if (dichVu == null)
            {
                return NotFound();
            }
            return View(dichVu);
        }

        // POST: DichVus/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TenDV,Mota,Gia,Ngungban")] DichVu dichVu, [Bind("files")] List<IFormFile> files)
        {
            if (id != dichVu.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (files != null && files.Count > 0)
                    {
                        var lstImage = _context.HinhDVs.Where(x => x.DichVuId == dichVu.Id);
                        if (lstImage.Any())
                        {
                            _context.HinhDVs.RemoveRange(lstImage);
                            foreach (var image in lstImage)
                            {
                                var path = Path.Combine(_webHostEnvironment.WebRootPath, "images/DichVu");
                                var pathFile = Path.Combine(path, image.Url);
                                if (System.IO.File.Exists(pathFile))
                                {
                                    System.IO.File.Delete(pathFile);
                                }
                            }
                        }
                        foreach (var file in files)
                        {
                            var folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/DichVu");
                            var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
                            var filePath = Path.Combine(folder, fileName);

                            var image = new HinhDV
                            {
                                DichVu = dichVu,
                                DichVuId = dichVu.Id,
                                Url = fileName,
                            };
                            dichVu.HinhDVs.Add(image);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }
                        }
                    }
                    _context.Update(dichVu);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DichVuExists(dichVu.Id))
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
            return View(dichVu);
        }

        // GET: DichVus/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var dichVu = await _context.DichVus
                .FirstOrDefaultAsync(m => m.Id == id);
            if (dichVu == null)
            {
                return NotFound();
            }

            return View(dichVu);
        }

        // POST: DichVus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dichVu = await _context.DichVus.FindAsync(id);
            if (dichVu != null)
            {
                _context.DichVus.Remove(dichVu);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DichVuExists(int id)
        {
            return _context.DichVus.Any(e => e.Id == id);
        }
    }
}
