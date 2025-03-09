using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class UserListViewModel
    {
        public IEnumerable<User> Users { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; } // Thêm thuộc tính SearchQuery
    }
}
