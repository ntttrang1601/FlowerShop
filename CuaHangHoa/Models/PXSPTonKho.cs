namespace CuaHangHoa.Models
{
    public class PXSPTonKho
    {
        public int Id { get; set; }
        public string StaffId { get; set; }
        public User? Staff { get; set; }
        public int TongSoLuong { get; set; }
        public DateTime NgayXuat { get; set; }
        public string? GhiChu { get; set; }

        public ICollection<CTPXSPTonKho> CTPXSPTonKhos { get; set; } = new List<CTPXSPTonKho>();
    }
}
