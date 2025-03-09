using Microsoft.AspNetCore.Identity;
using System.Net;

namespace CuaHangHoa.Models
{
	public class User:IdentityUser
	{
		public DateTime CreateTime { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string? City { get; set; }
        public DateTime? DeletedAt { get; set; } = null;

        public ICollection<DiaChi> DiaChis { get; set; } = new List<DiaChi>();
		public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; } = new List<ChiTietGioHang>();
		public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
        public ICollection<DangKyDichVu> DangKyDichVus { get; set; } = new List<DangKyDichVu>();
        public ICollection<PhieuNhap> PhieuNhaps { get; set; } = new List<PhieuNhap>();
        public ICollection<PhieuXuat> PhieuXuats { get; set; } = new List<PhieuXuat>();
        public ICollection<PXSPTonKho> PXSPTonKhos { get; set; } = new List<PXSPTonKho>();

    }
}
