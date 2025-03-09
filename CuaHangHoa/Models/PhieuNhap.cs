namespace CuaHangHoa.Models
{
    public class PhieuNhap
    {
        public int Id { get; set; }
        public int NhaCungCapId { get; set; }
        public NhaCungCap? NhaCungCap { get; set; }
        public string UserId { get; set; }
        public User? User { get; set; }
        public DateTime NgayNhap {  get; set; } 
        public int SoLuongMatHang {  get; set; }
        public double TongTien {  get; set; }
        public string? GhiChu {  get; set; }
        public bool IsDeleted { get; set; } = false; // Mặc định là chưa bị xóa

        public ICollection<CTPhieuNhap> CTPhieuNhaps { get; set; } = new List<CTPhieuNhap>();
    }
}
