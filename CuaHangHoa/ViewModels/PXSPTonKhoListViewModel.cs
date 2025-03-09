using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class PXSPTonKhoListViewModel
    {
        public List<PXSPTonKho> PXSPTonKhos { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
