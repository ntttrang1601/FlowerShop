namespace CuaHangHoa.Models
{
    public class Hinh
    {
        public int Id { get; set; }
        public int SanPhamId { get; set; }
        public string Url { get; set; }

        public SanPham? SanPham { get; set; }
    }
}
