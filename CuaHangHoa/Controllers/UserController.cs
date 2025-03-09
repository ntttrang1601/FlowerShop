using CuaHangHoa.Data;
using CuaHangHoa.Models;
using CuaHangHoa.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CuaHangHoa.Controllers
{
    public class UserController : Controller
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        public UserController(MyDbContext context, UserManager<User> userManager, SignInManager<User> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Account()
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = user.Value;
            User u =_context.Users.Where (x => x.Id == userId).FirstOrDefault();
            UserViewModel uv= new UserViewModel();
            uv.FirstName = u.FirstName;
            uv.LastName = u.LastName;
            uv.FullName = u.LastName + " " + u.FirstName;
            uv.City = u.City;
            uv.CreateTime = u.CreateTime;
            uv.Email = u.Email;
            uv.PhoneNumber =u.PhoneNumber;
            return View(uv);
            
        }

		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult SubmitReview(int donHangId, int diem, string nhanXet)
		{
			var donHang = _context.DonHangs.Include(d => d.DanhGia).FirstOrDefault(d => d.Id == donHangId);

			if (donHang == null)
			{
				return NotFound();
			}

			if (donHang.DanhGia != null)
			{
				// Đơn hàng đã được đánh giá
				TempData["Error"] = "Đơn hàng này đã được đánh giá.";
				return RedirectToAction("OrderDetails", new { id = donHangId });
			}

			// Tạo đánh giá mới
			var danhGia = new DanhGia
			{
				DonHangId = donHangId,
				Diem = diem,
				NhanXet = nhanXet,
				NgayDanhGia = DateTime.Now // Lưu thời gian hiện tại
			};
			_context.DanhGias.Add(danhGia);
			_context.SaveChanges();
			if (diem < 1 || diem > 5)
			{
				TempData["Error"] = "Điểm đánh giá không hợp lệ.";
				return RedirectToAction("OrderDetails", new { id = donHangId });
			}
			TempData["Success"] = "Đánh giá của bạn đã được gửi.";
			return RedirectToAction("OrderDetails", new { id = donHangId });
		}


		//public IActionResult OrderList()
		//{
		//    var u = User.FindFirst(ClaimTypes.NameIdentifier);
		//    if (u == null)
		//    {
		//        return RedirectToAction("Login", "Account");
		//    }
		//    string userId = u.Value;
		//    User user = _context.Users.Where(x => x.Id == userId).First();
		//    ViewBag.FullName = user.LastName + " " + user.FirstName;
		//    string id = user.Id;
		//    List<DonHang> orders = _context.DonHangs.Where(x => x.UserId == id).ToList();
		//    List<OrderViewModel> ordersvm = new List<OrderViewModel>();
		//    foreach (DonHang o in orders)
		//    {
		//        OrderViewModel i = new OrderViewModel();
		//        i.Id = o.Id;
		//        i.Total = o.TongTien;
		//        i.Status = o.TenTrangThai;
		//        i.Date = o.NgayTao;
		//        i.PayMethod = o.TenCachThanhToan;
		//        int pId = _context.ChiTietDHs.Where(x => x.DonHangId == o.Id).First().SanPhamId;
		//        string img = _context.Hinhs.Where(x => x.SanPhamId == pId).First().Url;
		//        i.Images = img;
		//        ordersvm.Add(i);
		//    }
		//    return View(ordersvm);
		//}

		public IActionResult OrderList()
		{
			var u = User.FindFirst(ClaimTypes.NameIdentifier);
			if (u == null)
			{
				return RedirectToAction("Login", "Account");
			}

			string userId = u.Value;
			User user = _context.Users.FirstOrDefault(x => x.Id == userId);
			if (user == null)
			{
				return RedirectToAction("Login", "Account");
			}

			ViewBag.FullName = user.LastName + " " + user.FirstName;
			string id = user.Id;

			List<DonHang> orders = _context.DonHangs.Where(x => x.UserId == id).OrderByDescending(x => x.NgayTao).ToList();
			List<OrderViewModel> ordersvm = new List<OrderViewModel>();

			foreach (DonHang o in orders)
			{
				OrderViewModel i = new OrderViewModel
				{
					Id = o.Id,
					Total = o.TongTien,
					Status = o.TenTrangThai,
					Date = o.NgayTao,
					PayMethod = o.TenCachThanhToan
				};

				// Sử dụng FirstOrDefault để tránh lỗi nếu không có ChiTietDH nào
				var chiTietDH = _context.ChiTietDHs.FirstOrDefault(x => x.DonHangId == o.Id);
				if (chiTietDH != null) // Nếu tìm thấy ChiTietDH
				{
					int pId = chiTietDH.SanPhamId;

					// Kiểm tra Hinh
					var hinh = _context.Hinhs.FirstOrDefault(x => x.SanPhamId == pId);
					if (hinh != null) // Nếu tìm thấy Hinh
					{
						i.Images = hinh.Url;
					}
					else
					{
						// Gán ảnh mặc định nếu không có hình
						i.Images = "default-image.png"; // hoặc một URL hình ảnh mặc định nào đó
					}
				}
				else
				{
					// Xử lý nếu không có ChiTietDH (có thể bỏ qua hoặc gán giá trị mặc định)
					i.Images = "default-image.png"; // hoặc để trống nếu không có sản phẩm
				}

				ordersvm.Add(i);
			}

			return View(ordersvm);
		}


		public IActionResult OrderDetails(int id)
        {
            var u = User.FindFirst(ClaimTypes.NameIdentifier);
            if (u == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = u.Value;
            User user = _context.Users.Where(x => x.Id == userId).First();
            ViewBag.FullName = user.LastName + " " + user.FirstName;
            DonHang o = _context.DonHangs.Where(x => x.Id == id).First();
            var ords = _context.ChiTietDHs.Where(x => x.DonHangId == id).Include(x => x.SanPham).Include(x => x.SanPham.Hinhs).ToList();
			var danhGia = _context.DanhGias.FirstOrDefault(x => x.DonHangId == id);
			ViewBag.DanhGia = danhGia;
			ViewBag.DonHang = o;
            return View(ords);
        }

        public IActionResult Edit()
        {
            var user = User.FindFirst(ClaimTypes.NameIdentifier);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = user.Value;
            User u = _context.Users.Where(x => x.Id == userId).First();
            UserViewModel userv = new UserViewModel();
            userv.FirstName = u.FirstName;
            userv.LastName = u.LastName;
            userv.FullName = userv.LastName + " " + userv.FirstName;
            userv.City = u.City;
            userv.CreateTime = u.CreateTime;
            userv.Email = u.Email;
            userv.PhoneNumber = u.PhoneNumber;
            return View(userv);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel model)
        {
            var u = User.FindFirst(ClaimTypes.NameIdentifier);
            if (u == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = u.Value;
            User user = _context.Users.Where(x => x.Id == userId).FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.City = model.City;
            user.PhoneNumber = model.PhoneNumber;
            _context.Update(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Account));
        }

        public IActionResult ChangePassword()
        {
            var u = User.FindFirst(ClaimTypes.NameIdentifier);
            if (u == null)
            {
                return RedirectToAction("Login", "Account");
            }
            string userId = u.Value;
            User user = _context.Users.Where(x => x.Id == userId).First();
            ViewBag.FullName = user.LastName + " " + user.FirstName;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(PasswordViewModel model)
        {

            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                // ChangePasswordAsync changes the user password
                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

                // The new password did not meet the complexity rules or
                // the current password is incorrect. Add these errors to
                // the ModelState and rerender ChangePassword view
                if (!result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View();
                }

                // Upon successfully changing the password refresh sign-in cookie
                await _signInManager.RefreshSignInAsync(user);
                return RedirectToAction(nameof(Account));
            }

            return View(model);
        }
    }
}
