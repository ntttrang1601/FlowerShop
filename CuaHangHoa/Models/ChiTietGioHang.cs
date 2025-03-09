namespace CuaHangHoa.Models
{
    public class ChiTietGioHang
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }

        public string UserId { get; set; }

        public User User { get; set; }
        public int SoLuong {  get; set; }
        
        public SanPham? SanPham { get; set; }

    }
}
