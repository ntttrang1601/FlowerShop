using System.ComponentModel.DataAnnotations;

namespace CuaHangHoa.Models
{
    public class Tinh
    {
        [Key]
        public string province_id { get; set; }
        public string province_name { get; set; }
        public string province_type { get; set; }
    }
}
