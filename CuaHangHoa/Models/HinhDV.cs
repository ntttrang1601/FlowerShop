namespace CuaHangHoa.Models
{
    public class HinhDV
    {
        public int Id { get; set; }
        public int DichVuId { get; set; }
        public string Url { get; set; }

        public DichVu? DichVu { get; set; }
    }
}
