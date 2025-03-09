using CuaHangHoa.Data;
using CuaHangHoa.Models;
using CuaHangHoa.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CuaHangHoa.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class CustomersController : Controller
    {
        private readonly MyDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public CustomersController(MyDbContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            int pageSize = 10; // Mỗi trang sẽ hiển thị 10 dòng
            string roleName = "User"; // Vai trò cần lọc

            // Truy vấn danh sách người dùng có vai trò "User"
            var query = from user in _context.Users
                        join userRole in _context.UserRoles on user.Id equals userRole.UserId
                        join role in _context.Roles on userRole.RoleId equals role.Id
                        where role.Name == roleName 
                        select user;
            // Lọc theo từ khóa tìm kiếm nếu có
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(user => user.FirstName.Contains(searchQuery) || user.LastName.Contains(searchQuery) || user.Email.Contains(searchQuery));
            }


            // Tính tổng số người dùng trong vai trò "User"
            int totalItems = await query.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            // Lấy dữ liệu người dùng của trang hiện tại
            var users = await query
                .OrderByDescending(u => u.CreateTime) // Sắp xếp theo thời gian tạo
                .Skip((page - 1) * pageSize) // Bỏ qua các dòng ở các trang trước
                .Take(pageSize) // Lấy số lượng dòng cần thiết cho trang hiện tại
                .ToListAsync();

            // Truyền dữ liệu sang View
            var viewModel = new UserListViewModel
            {
                Users = users,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchQuery = searchQuery
            };

            return View(viewModel);
        }
        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string searchQuery)
        {
            // Kiểm tra nếu searchQuery là null hoặc rỗng
            if (string.IsNullOrEmpty(searchQuery))
            {
                return Json(new List<string>()); // Trả về danh sách trống nếu không có tìm kiếm
            }

            // Lọc người dùng theo từ khóa tìm kiếm
            var suggestions = await _context.Users
                .Where(u => u.FirstName.Contains(searchQuery) || u.LastName.Contains(searchQuery) || u.Email.Contains(searchQuery))
                .Take(5) // Giới hạn số lượng gợi ý
                .Select(u => new { u.FirstName, u.LastName }) // Chỉ trả về FirstName và LastName
                .ToListAsync();

            // Kiểm tra nếu gợi ý trả về là null hoặc rỗng
            if (suggestions == null || !suggestions.Any())
            {
                return Json(new List<string>());
            }

            // Trả về danh sách các gợi ý tìm kiếm dưới dạng JSON
            return Json(suggestions);
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
                LastName = lastName,
                FirstName = firstName,
                UserName = username,
                CreateTime = DateTime.Now,
                City = null, // City không bắt buộc, có thể để null hoặc không gán gì
                Email = email
            };

            // Tạo người dùng trong hệ thống Identity
            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (result.Succeeded)
            {
                // Gán role "User" cho người dùng mới
                await _userManager.AddToRoleAsync(user, "User");
                return RedirectToAction("Index");
            }

            // Xử lý lỗi nếu có
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View();
        }

        // Phương thức GET: Hiển thị trang xác nhận xóa
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



        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null )
            {
                return NotFound();
            }

            var model = new EditUserViewModel
            {
                Id = user.Id,  // Không chỉnh sửa Id
                FirstName = user.FirstName,
                LastName = user.LastName,
                City = user.City,
                PhoneNumber=user.PhoneNumber
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




    }
}
