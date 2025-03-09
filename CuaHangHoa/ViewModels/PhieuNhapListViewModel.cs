using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class PhieuNhapListViewModel
    {
        public IEnumerable<PhieuNhap> PhieuNhaps { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
