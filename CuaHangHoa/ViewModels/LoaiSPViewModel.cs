using CuaHangHoa.Models;
using X.PagedList;

namespace CuaHangHoa.ViewModels
{
    public class LoaiSPViewModel
    {
        public List<LoaiSP> LoaiSPs { get; set; }
        public IPagedList<SanPhamViewModel> SanPhams { get; set; } // Cập nhật kiểu danh sách sản phẩm
    }
}
