using CuaHangHoa.Models;
using System.ComponentModel.DataAnnotations;

namespace CuaHangHoa.ViewModels
{
    public class DKDichVuViewModel
    {
        public int Id { get; set; }
        public int DichVuId { get; set; }
        public string TenDV {  get; set; }
        public DateTime NgayDangKy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime NgayToChuc { get; set; }
        public string DchiToChuc { get; set; }
        public string? GhiChu { get; set; }
        public TrangThaiDK TrangThaiDangKy { get; set; }
        public string Images { get; set; }
    }
}
