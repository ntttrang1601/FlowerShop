namespace CuaHangHoa.Models
{
    public class LoaiSP
    {
        public int Id { get; set; }
        public string TenLoai { get; set; }
        public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();

    }
}
