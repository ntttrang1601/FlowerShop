namespace CuaHangHoa.Models
{
    public class DanhGia
    {
        public int Id { get; set; }
        public int DonHangId { get; set; }
        public DonHang DonHang { get; set; }
        public int Diem {  get; set; }
        public string NhanXet {  get; set; }
        public DateTime NgayDanhGia { get; set; }

    }
}
