using CuaHangHoa.Data;
using CuaHangHoa.Models;
using CuaHangHoa.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CuaHangHoa.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly MyDbContext _context;
        public HomeController(MyDbContext context)
        {
            _context = context;
        }
        
        public IActionResult Index()
        {
            var viewModel = new SPViewModel
            {
                SanPhams = _context.SanPhams
                    .OrderBy(x => Guid.NewGuid()) 
                    .Take(8)                      
                    .ToList(),
                HinhCuaSPs = _context.Hinhs
                    .Include(h => h.SanPham)
                    .Where(h => h.SanPham != null && h.SanPham.Ngungban==false)
                    .GroupBy(img => img.SanPham.Id)
                    .Select(group => group.FirstOrDefault())
                    .OrderBy(x => Guid.NewGuid()) 
                    .Take(8)                      
                    .ToList()
            };
            return View(viewModel);
        }



        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Blog()
        {
            return View();
        }
        public IActionResult AboutUs()
        {
            return View();
        }
        public IActionResult Contact() {
            
            return View();
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]       
        public IActionResult SubmitContactForm([Bind("HoTen,SDT,MatHang,ThongDiep")] LienHe lienHe)
        {
            if (ModelState.IsValid)
            {
                lienHe.NgayGui = DateTime.Now;
                lienHe.TTLienHe = TrangThaiLH.ChuaTuVan;
                lienHe.GhiChu = "";
                _context.LienHes.Add(lienHe);
                _context.SaveChanges();

                // Trả về JSON thông báo gửi thành công
                return Json(new { success = true });
            }

            // Nếu có lỗi, trả về JSON thông báo lỗi
            return Json(new { success = false, message = "Có lỗi xảy ra. Vui lòng kiểm tra thông tin." });
        }


    }
}
