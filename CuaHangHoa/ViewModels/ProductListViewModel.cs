using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class ProductListViewModel
    {
        public IEnumerable<SanPham> SanPhams { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; } // Từ khóa tìm kiếm
    }
}
