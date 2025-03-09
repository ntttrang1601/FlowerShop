using CuaHangHoa.Data;
using CuaHangHoa.Models;
using CuaHangHoa.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin,Staff")]

    public class PXSPTonKhoController : Controller
    {
        private readonly MyDbContext _context;
        private readonly int _pageSize = 10;
        public PXSPTonKhoController(MyDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int page = 1)
        {
            var query = _context.PXSPTonKhos
                .Include(p => p.Staff)
                .Include(p => p.CTPXSPTonKhos)
                .ThenInclude(ct => ct.SanPham)
                .OrderByDescending(p => p.NgayXuat);

            // Tổng số phiếu xuất tồn kho
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Lấy danh sách phiếu xuất tồn kho cho trang hiện tại
            var pxspTonKhos = await query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToListAsync();

            // Tạo ViewModel chứa dữ liệu phân trang
            var viewModel = new PXSPTonKhoListViewModel
            {
                PXSPTonKhos = pxspTonKhos,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }
        public IActionResult Create()
        {
            ViewBag.LoaiSPs = new SelectList(_context.LoaiSPs, "Id", "TenLoai");

            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> Create(PXSPTonKho phieuXuat)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            phieuXuat.StaffId = userId;

            // Lấy UserId từ người dùng hiện tại

            if (string.IsNullOrEmpty(phieuXuat.GhiChu))
            {
                phieuXuat.GhiChu = "Không có ghi chú";
            }

            phieuXuat.TongSoLuong = phieuXuat.CTPXSPTonKhos.Sum(ct => ct.SoLuong);

            _context.PXSPTonKhos.Add(phieuXuat);

            foreach (var ctPhieuXuat in phieuXuat.CTPXSPTonKhos)
            {
                var sanPham = await _context.SanPhams.FindAsync(ctPhieuXuat.SanPhamId);
                if (sanPham != null)
                {
                    sanPham.Soluongkho -= ctPhieuXuat.SoLuong;
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
                ViewBag.LoaiSPs = new SelectList(_context.LoaiSPs.ToList(), "Id", "TenLoai");
                return View(phieuXuat);
            }

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

        public IActionResult Details(int id)
        {
            var phieu = _context.PXSPTonKhos.Include(p => p.Staff)
                                             .Include(p => p.CTPXSPTonKhos)
                                             .ThenInclude(ct => ct.SanPham)
                                             .FirstOrDefault(p => p.Id == id);
            if (phieu == null)
            {
                return NotFound();
            }
            return View(phieu);
        }

    }
}
