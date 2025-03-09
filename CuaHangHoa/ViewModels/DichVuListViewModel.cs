using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class DichVuListViewModel
    {
        public IEnumerable<DichVu> DichVus { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string SearchQuery { get; set; } // Từ khóa tìm kiếm
    }
}
