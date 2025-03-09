using CuaHangHoa.Data;
using CuaHangHoa.Models;
using CuaHangHoa.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.Security.Claims;

namespace CuaHangHoa.Controllers
{
    [AllowAnonymous]
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly HttpClient _httpClient;
        private readonly MyDbContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RoleManager<IdentityRole> roleManager, HttpClient httpClient, MyDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _httpClient = httpClient;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {         
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.UserName);
                // Kiểm tra nếu tài khoản không tồn tại hoặc bị khóa
                if (user == null || user.DeletedAt != null)
                {
                    ModelState.AddModelError("", "Tài khoản không tồn tại hoặc đã bị khóa");
                    return View(model);
                }



                var result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, false, false);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Đăng nhập thành công";
                    if (User.IsInRole("Admin") || User.IsInRole("Staff"))
                    {
                        return RedirectToAction("Index", "LienHes");
                    }
                    return RedirectToAction("Index", "Home");

                }
                ModelState.AddModelError("", "Thông tin đăng nhập không hợp lệ");
                return View(model);

            }
            
            return View(model);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUserByUsername = await _userManager.FindByNameAsync(model.UserName);
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                // Kiểm tra nếu đã có Email
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                {
                    ModelState.AddModelError("Email", "Email đã được sử dụng.");
                    return View(model);
                }
                User user = new()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.UserName,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    City = model.City,
                    CreateTime = DateTime.Now,

                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = "Tạo tài khoản thành công, hãy đăng nhập!";
                    var roleExists = await _roleManager.RoleExistsAsync("User");
                    if (!roleExists)
                    {
                        await _roleManager.CreateAsync(new IdentityRole("User"));
                    }

                    await _userManager.AddToRoleAsync(user, "User");

                    return RedirectToAction("Login", "Account");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        public async Task GoogleLogin()
        {
            await HttpContext.ChallengeAsync(GoogleDefaults.AuthenticationScheme,
                new AuthenticationProperties
                {
                    RedirectUri = Url.Action("GoogleResponse")
                });

        }
        public async Task<IActionResult> GoogleResponse()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (result.Succeeded)
            {
                var claims = result.Principal.Identities.FirstOrDefault().Claims;
                var id = claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                var r = await _signInManager.ExternalLoginSignInAsync("Google", id, isPersistent: false, bypassTwoFactor: false);
                if (r.Succeeded)
                {
                    //Tài khoản có liên kết Google

                    var user = await _userManager.FindByLoginAsync("Google", id);
                    if (user != null)
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    //Tài khoản chưa tồn tại, thêm tài khoản và liên kết
                    var Email = claims.First(c => c.Type == ClaimTypes.Email).Value;
                    var FirstName = claims.First(c => c.Type == ClaimTypes.GivenName).Value;
                    var LastName = claims.First(c => c.Type == ClaimTypes.Surname).Value;
                    return RedirectToAction("ExternalInfo", new { email = Email, fName = FirstName, lName = LastName, id = id });

                }

            }
            return RedirectToAction("Login");

        }

        public IActionResult ExternalInfo(string email, string fName, string lName, string id)
        {
            ExternalLoginModel model = new ExternalLoginModel
            {
                Id = id,
                Email = email,
                FirstName = fName,
                LastName = lName,
                PhoneNumber = "",
                City = ""
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ExternalInfo(ExternalLoginModel model)
        {
            if (ModelState.IsValid)
            {
                User user = new()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    Email = model.Email,
                    CreateTime = DateTime.Now,
                    City = model.City
                };
                var kq = await _userManager.CreateAsync(user);
                UserLoginInfo info = new UserLoginInfo("Google", model.Id, "Google");
                var kq1 = await _userManager.AddLoginAsync(user, info);
                if (kq.Succeeded && kq1.Succeeded)
                {
                    // Đăng nhập người dùng mới vào hệ thống
                    await _signInManager.SignInAsync(user, isPersistent: false);

                    return RedirectToAction("Index", "Home");
                }
            }
            return View(model);
        }
    }
}
