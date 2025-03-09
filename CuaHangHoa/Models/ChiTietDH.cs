using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace CuaHangHoa.Models
{
    [PrimaryKey(nameof(DonHangId), nameof(SanPhamId))]
    public class ChiTietDH
    {
        public int DonHangId { get; set; }
        public int SanPhamId { get; set; }
        public int SoLuong { get; set; }
        public double TongGia { get; set; }
        public double GiaSP { get; set; }
        public DonHang DonHang { get; set; }
        public SanPham SanPham { get; set; }

    }
}
