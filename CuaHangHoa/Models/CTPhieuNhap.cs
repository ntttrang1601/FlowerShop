namespace CuaHangHoa.Models
{
    public class CTPhieuNhap
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }
        public int PhieuNhapId {  get; set; }
        public PhieuNhap? PhieuNhap { get; set; }
        public int SoLuong {  get; set; }
        public double GiaNhap {  get; set; }
    }
}
