using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class DangKyDichVuListViewModel
    {
        public IEnumerable<DangKyDichVu> DangKyDichVus { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
