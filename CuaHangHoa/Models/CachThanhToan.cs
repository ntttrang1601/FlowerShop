using System.ComponentModel.DataAnnotations;

namespace CuaHangHoa.Models
{
    public class CachThanhToan
    {
        [Key]
        public string Ten { get; set; }
        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}
