using System.ComponentModel.DataAnnotations;

namespace CuaHangHoa.ViewModels
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Nhập tên tài khoản.")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
    }
}
