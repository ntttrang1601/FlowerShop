using CuaHangHoa.Data;
using CuaHangHoa.Models;
using CuaHangHoa.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using X.PagedList;
using X.PagedList.Extensions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CuaHangHoa.Controllers
{
    public class ProductController : Controller
    {
        private readonly MyDbContext _dbContext;
        public ProductController(MyDbContext dbContext)
        {
            _dbContext = dbContext;
        }


        public IActionResult Index(string search, int? page)
        {
            int pageNumber = page ?? 1; // Số trang hiện tại, mặc định là 1
            int pageSize = 10; // Số mục trên mỗi trang

            var query = _dbContext.SanPhams
            .Include(sp => sp.Hinhs)
            .Where(sp => sp.Ngungban == false && sp.Soluongkho > 0);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(sp => sp.Ten.Contains(search) || sp.Mota.Contains(search));
            }

            var sanPhams = query.Select(sp => new SanPhamViewModel
            {
                Id = sp.Id,
                Ten = sp.Ten,
                Dongia = sp.Dongia,
                GiaSauGiamGia = sp.PhanTramGiamGia.HasValue ? sp.Dongia * (1 - sp.PhanTramGiamGia.Value / 100) : sp.Dongia,
                PhanTramGiamGia = sp.PhanTramGiamGia,
                Hinh = sp.Hinhs.FirstOrDefault() != null ? sp.Hinhs.FirstOrDefault().Url : "no-image.png"
            })
                .Distinct().OrderByDescending(sp => sp.Id)
                .ToPagedList(pageNumber, pageSize); // Áp dụng phân trang

            var viewModel = new LoaiSPViewModel
            {
                LoaiSPs = _dbContext.LoaiSPs.ToList(),
                SanPhams = sanPhams // Sử dụng IPagedList thay vì List
            };

            ViewBag.SearchKeyword = search;
            return View(viewModel);
        }
        //public IActionResult GetProductsByCategory(int categoryId)
        //{
        //    var products = _dbContext.SanPhams
        //        .Include(sp => sp.Hinhs)  // Đảm bảo bạn đã Include các hình ảnh
        //        .Where(sp => sp.LoaiSPId == categoryId && sp.Ngungban==false && sp.Soluongkho>0)
        //        .Select(sp => new
        //        {
        //            id = sp.Id,
        //            ten = sp.Ten,
        //            dongia = sp.Dongia,
        //            giaSauGiamGia = sp.PhanTramGiamGia.HasValue ? sp.Dongia * (1 - sp.PhanTramGiamGia.Value / 100) : sp.Dongia,
        //            phanTramGiamGia = sp.PhanTramGiamGia,
        //            hinh = sp.Hinhs != null && sp.Hinhs.Any() ? sp.Hinhs.FirstOrDefault().Url : null // Kiểm tra trước khi lấy giá trị
        //        }).ToList();

        //    return Json(products);
        //}
        public JsonResult GetProductsByCategory(int categoryId)
        {
            var products = _dbContext.SanPhams
                .Where(p => p.LoaiSPId == categoryId)
                .Select(p => new
                {
                    id = p.Id,
                    ten = p.Ten,
                    dongia = p.Dongia,
                    hinh = p.Hinhs != null && p.Hinhs.Any() ? p.Hinhs.FirstOrDefault().Url : null, // Kiểm tra trước khi lấy giá trị,
                    phanTramGiamGia = p.PhanTramGiamGia,
                    GiaSauGiamGia = p.PhanTramGiamGia > 0 ? Math.Round(p.Dongia * (1 - (double)p.PhanTramGiamGia / 100)) : p.Dongia
                })
                .ToList();

            return Json(products);
        }




        [HttpGet]
        public async Task<IActionResult> ProductDetail(int id)
        {
            var product = await _dbContext.SanPhams
                .Include(e => e.Hinhs)
                .Include(e => e.LoaiSP)
                .SingleOrDefaultAsync(e => e.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            else
            {
                return View(product);
            }
        }


    }
}
