using System.ComponentModel.DataAnnotations;

namespace CuaHangHoa.ViewModels
{
    public class PasswordViewModel
    {
        [Required(ErrorMessage = "Nhập mật khẩu cũ.")]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Nhập mật khẩu mới.")]
        [StringLength(100, ErrorMessage = "Dài ít nhất 2 ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Nhập lại mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "Mật khẩu mới và mật khẩu nhập lại không khớp.")]
        public string ConfirmPassword { get; set; }
    }
}
