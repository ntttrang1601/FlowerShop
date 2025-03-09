using System.ComponentModel.DataAnnotations;

namespace CuaHangHoa.Models
{
    public class TrangThaiDH
    {
        [Key]
        public string Ten { get; set; }

        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
    }
}
