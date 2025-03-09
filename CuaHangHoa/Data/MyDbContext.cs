using CuaHangHoa.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CuaHangHoa.ViewModels;

namespace CuaHangHoa.Data
{
	public class MyDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
 
        public virtual DbSet<LoaiSP>LoaiSPs { get; set; }
        public virtual DbSet<NhaCungCap>NhaCungCaps { get; set; }
        public virtual DbSet<SanPham>SanPhams { get; set; }
        public virtual DbSet<DiaChi>DiaChis { get; set; }

        public virtual DbSet<Hinh>Hinhs { get; set; }
        public virtual DbSet<DonHang>DonHangs { get; set; }
        public virtual DbSet<ChiTietDH> ChiTietDHs { get; set; }
        public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; }
        public virtual DbSet<CachThanhToan> CachThanhToans { get; set; }

        public virtual DbSet<TrangThaiDH> TrangThaiDHs { get; set; }
        public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public virtual DbSet<CTPhieuNhap> CTPhieuNhaps { get; set; }
        public virtual DbSet<DichVu> DichVus { get; set; }

        public virtual DbSet<DangKyDichVu> DangKyDichVus { get; set; }
        public virtual DbSet<HinhDV> HinhDVs { get; set; }
        public virtual DbSet<PhieuXuat> PhieuXuats { get; set; }
        public virtual DbSet<CTPhieuXuat> CTPhieuXuats { get; set; }
        public virtual DbSet<LienHe> LienHes { get; set; }
        public virtual DbSet<PXSPTonKho> PXSPTonKhos { get; set; }
        public virtual DbSet<CTPXSPTonKho> CTPXSPTonKhos { get; set; }
        public virtual DbSet<DanhGia> DanhGias { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Gọi phương thức OnModelCreating của lớp cơ sở để thiết lập các thực thể liên quan đến Identity
            base.OnModelCreating(modelBuilder);

            // Thiết lập quan hệ giữa PhieuNhap và CTPhieuNhap
            modelBuilder.Entity<CTPhieuNhap>()
                .HasOne(ct => ct.PhieuNhap)
                .WithMany(pn => pn.CTPhieuNhaps)
                .HasForeignKey(ct => ct.PhieuNhapId);
            modelBuilder.Entity<PhieuXuat>()
                .HasOne(a => a.DangKyDichVu)
                .WithMany(s => s.PhieuXuat)
                .HasForeignKey(a => a.DangKyDichVuId)
                .OnDelete(DeleteBehavior.Restrict);
        }
	    public DbSet<CuaHangHoa.ViewModels.EditUserViewModel> EditUserViewModel { get; set; } = default!;
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.LogTo(Console.WriteLine); // Hiển thị truy vấn SQL
        }

    }
}
