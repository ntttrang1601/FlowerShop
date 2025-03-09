using System.ComponentModel.DataAnnotations;

namespace CuaHangHoa.ViewModels
{
    public class CreateUserViewModel
    {
        [Required]
        [Display(Name = "Họ")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Tên")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Tên người dùng")]
        public string UserName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
