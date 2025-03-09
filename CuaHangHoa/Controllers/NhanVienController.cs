using CuaHangHoa.Data;
using CuaHangHoa.Models;
using CuaHangHoa.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin")]
    public class NhanVienController : Controller
    {
        
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly int _pageSize = 10;
        public NhanVienController(MyDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }
        //public IActionResult Index()
        //{
        //    // Tên của vai trò bạn muốn lọc
        //    string roleName = "Staff";

        //    var usersInRole = (from user in _context.Users
        //                       join userRole in _context.UserRoles on user.Id equals userRole.UserId
        //                       join role in _context.Roles on userRole.RoleId equals role.Id
        //                       where role.Name == roleName
        //                       select user).OrderByDescending(p=>p.CreateTime).ToList();

        //    return View(usersInRole);
        //    //return View();
        //}

        public IActionResult Index(int page = 1)
        {
            // Tên vai trò bạn muốn lọc
            string roleName = "Staff";

            var query = (from user in _context.Users
                         join userRole in _context.UserRoles on user.Id equals userRole.UserId
                         join role in _context.Roles on userRole.RoleId equals role.Id
                         where role.Name == roleName
                         select user).OrderByDescending(p => p.CreateTime);

            // Tổng số người dùng trong vai trò
            var totalItems = query.Count();
            var totalPages = (int)Math.Ceiling(totalItems / (double)_pageSize);

            // Lấy các người dùng trong trang hiện tại
            var usersInRole = query
                .Skip((page - 1) * _pageSize)
                .Take(_pageSize)
                .ToList();

            // Tạo view model chứa dữ liệu phân trang
            var viewModel = new UserListViewModel
            {
                Users = usersInRole,
                CurrentPage = page,
                TotalPages = totalPages
            };

            return View(viewModel);
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string firstName, string lastName, string username, string email)
        {
            // Mặc định mật khẩu
            string defaultPassword = "Abc123@";

            // Kiểm tra xem username hoặc email có bị trùng không
            if (await _userManager.FindByNameAsync(username) != null)
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
            }

            if (await _userManager.FindByEmailAsync(email) != null)
            {
                ModelState.AddModelError("Email", "Email đã tồn tại.");
            }

            // Nếu có lỗi, trả về View với thông báo lỗi
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Tạo người dùng mới với các thông tin cơ bản
            var user = new User
            {
                FirstName = firstName,
                LastName = lastName,
                UserName = username,
                CreateTime = DateTime.Now,
                City = null, // City không bắt buộc, có thể để null hoặc không gán gì
                Email = email
            };

            // Tạo người dùng trong hệ thống Identity
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (result.Succeeded)
            {               
                await _userManager.AddToRoleAsync(user, "Staff");
                return RedirectToAction("Index");
            }

            // Xử lý lỗi nếu có
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,  // Không chỉnh sửa Id
                FirstName = user.FirstName,
                LastName = user.LastName,
                City = user.City,
                PhoneNumber = user.PhoneNumber
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null || user.DeletedAt != null)
                {
                    return NotFound();
                }

                // Cập nhật thông tin (không chỉnh sửa Id)
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.City = model.City;
                user.PhoneNumber = model.PhoneNumber;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Index");  // Redirect về trang danh sách
                }

                // Xử lý lỗi nếu có
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Truyền id vào View
            return View(user);
        }

        // Phương thức POST: Xóa người dùng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.DeletedAt = DateTime.Now;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            // Xử lý lỗi nếu có
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Khôi phục tài khoản bằng cách đặt DeletedAt thành null
            user.DeletedAt = null;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Tài khoản đã được khôi phục thành công.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Index");
        }

    }
}
