namespace CuaHangHoa.Models
{
    public class NhaCungCap
    {
        public int Id { get; set; }
        public string Ten { get; set; }
        public string Dchi { get; set; }
        public string Sdt { get; set; }
        public string Email { get; set; }
        public string? Mota { get; set; }

        public ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();

    }
}
