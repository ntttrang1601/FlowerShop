namespace CuaHangHoa.Models
{
    public class PhieuXuat
    {
        public int Id { get; set; }
        public int DangKyDichVuId { get; set; }
        public DangKyDichVu? DangKyDichVu { get; set; }
        public string StaffId { get; set; }
        public User? Staff { get; set; }
        public int SoLuongMatHang {  get; set; }
        public DateTime NgayXuat { get; set; }
        public double PhiPhatSinh {  get; set; }
        public double TongTien {  get; set; }
        public string? GhiChu {  get; set; }
        public bool IsDeleted { get; set; } = false; // Mặc định là chưa bị xóa

        public ICollection<CTPhieuXuat> CTPhieuXuats { get; set; } = new List<CTPhieuXuat>();
    }
}
