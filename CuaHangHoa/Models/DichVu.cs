namespace CuaHangHoa.Models
{
    public class DichVu
    {
        public int Id { get; set; }
        public string TenDV {  get; set; }

        public string? Mota { get; set; }
        public string Gia {  get; set; }
        public bool Ngungban { get; set; }


        public ICollection<DangKyDichVu> DangKyDichVus { get; set; } = new List<DangKyDichVu>();
        public ICollection<HinhDV> HinhDVs { get; set; } = new List<HinhDV>();

    }
}
