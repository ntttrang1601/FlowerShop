namespace CuaHangHoa.Models
{
    public class SanPham
    {
        public int Id { get; set; }
        public string Ten { get; set; }
        public string? Mota { get; set; }
        public string Donvitinh { get; set; }
        public double Dongia { get; set; }
        public int Soluongkho { get; set; }
        public bool Ngungban { get; set; }
        public int LoaiSPId { get; set; }
        // Thêm trường giảm giá
        public double? PhanTramGiamGia { get; set; } // Phần trăm giảm giá (nếu có)

        // Tính giá sau giảm giá
        public double GiaSauGiamGia
        {
            get
            {
                // Nếu phần trăm giảm giá nằm ngoài phạm vi 0 - 100, bỏ qua giảm giá
                if (PhanTramGiamGia.HasValue && PhanTramGiamGia.Value >= 0 && PhanTramGiamGia.Value <= 100)
                {
                    return Dongia * (1 - PhanTramGiamGia.Value / 100);
                }
                return Dongia; // Không giảm giá
            }
        }


        public LoaiSP? LoaiSP { get; set; }
        public ICollection<Hinh> Hinhs { get; set; } = new List<Hinh>();
        public ICollection<ChiTietDH> ChiTietDHs { get; set; } = new List<ChiTietDH>();

        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
        public ICollection<CTPhieuNhap> CTPhieuNhaps { get; set; }=new List<CTPhieuNhap>();
        public ICollection<CTPhieuXuat> CTPhieuXuats { get; set; } = new List<CTPhieuXuat>();
        public ICollection<CTPXSPTonKho> CTPXSPTonKhos { get; set; } = new List<CTPXSPTonKho>();



    }
}
