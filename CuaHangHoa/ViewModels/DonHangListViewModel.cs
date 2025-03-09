using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class DonHangListViewModel
    {
        public IEnumerable<DonHang> DonHangs { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public List<DanhGia> DanhGias { get; set; }
    }
}
