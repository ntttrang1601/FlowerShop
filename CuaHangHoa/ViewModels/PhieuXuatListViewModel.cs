using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class PhieuXuatListViewModel
    {
        public IEnumerable<PhieuXuat> PhieuXuats { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
