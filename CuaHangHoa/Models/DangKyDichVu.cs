namespace CuaHangHoa.Models
{
    public enum TrangThaiDK { DangXuLy,DaHoanThanh,DaHuy, DaXacNhan }
    public class DangKyDichVu
    {
        public int Id {  get; set; }
        public int DichVuId {  get; set; }
        public DichVu? DichVu { get; set; }
        public string UserId {  get; set; }
        public User? User { get; set; }
        public DateTime NgayDangKy {  get; set; }
        public DateTime NgayToChuc {  get; set; }
        public TrangThaiDK TrangThaiDangKy { get; set;}
        public string DchiToChuc { get; set; }
        public string? GhiChu {  get; set; }
        public ICollection<PhieuXuat> PhieuXuat { get; set; } = new List<PhieuXuat>();


    }
}
