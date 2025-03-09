namespace CuaHangHoa.Models
{
    public enum TrangThaiLH { ChuaTuVan, DaTuVan }
    public class LienHe
    {
        public int Id { get; set; }
        public string HoTen { get; set; }
        public string SDT { get; set; }
        public string? MatHang { get; set; }
        public string ThongDiep { get; set; }
        public DateTime NgayGui {  get; set; }
        public TrangThaiLH TTLienHe { get; set; }
        public string? GhiChu { get; set; }
        public string? TenNhanVien { get; set; }
    }
}
