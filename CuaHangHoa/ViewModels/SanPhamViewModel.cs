namespace CuaHangHoa.ViewModels
{
    public class SanPhamViewModel
    {
        public int Id { get; set; }
        public string Ten { get; set; }
        public double Dongia { get; set; }
        public double GiaSauGiamGia { get; set; } // Giá sau giảm giá
        public double? PhanTramGiamGia { get; set; } // Phần trăm giảm giá
        public string Hinh { get; set; }
    }
}
