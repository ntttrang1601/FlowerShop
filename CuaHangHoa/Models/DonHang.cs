namespace CuaHangHoa.Models
{
    public class DonHang
    {
        public int Id { get; set; }
        public double TongTien { get; set; }
        public double PhiVanChuyen { get; set; }

        public string UserId { get; set; }
        public User? User { get; set; }
        public string TenCachThanhToan { get; set; }
        public CachThanhToan? CachThanhToan { get; set; }
        public bool DaThanhToan { get; set; } = false;
        public DateTime NgayTao { get; set; }
        public string TenTrangThai { get; set; }
        public TrangThaiDH? TrangThaiDH { get; set; }
        public string DchiGiaoHang { get; set; }
        public string NguoiNhan { get; set; }

        public ICollection<ChiTietDH> ChiTietDHs { get; set; } = new List<ChiTietDH>();
        public DanhGia? DanhGia { get; set; }


    }
}
