using System.ComponentModel.DataAnnotations;

namespace CuaHangHoa.ViewModels
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Nhập tên.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Nhập họ.")]
        public string? LastName { get; set; }

        [Required(ErrorMessage = "Nhập tên tài khoản.")]
        public string? UserName { get; set; }

        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Nhập email.")]
        [DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [DataType(DataType.MultilineText)]
        public string? City { get; set; }

        [Required(ErrorMessage = "Nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Compare("Password", ErrorMessage = "Mật khẩu khong khớp.")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }
    }
}
