namespace CuaHangHoa.Models
{
    public class CTPXSPTonKho
    {
        public int Id { get; set; }
        public int PXSPTonKhoId { get; set; }
        public PXSPTonKho? PXSPTonKho { get; set; }
        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }
        public int SoLuong { get; set; }
    }
}
