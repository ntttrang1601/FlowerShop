namespace CuaHangHoa.Models
{
    public class CTPhieuXuat
    {
        public int Id { get; set; }
        public int PhieuXuatId { get; set; }
        public PhieuXuat? PhieuXuat { get; set; }
        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set;}
        public int SoLuong {  get; set; }
        public double GiaSP {  get; set; }
    }
}
