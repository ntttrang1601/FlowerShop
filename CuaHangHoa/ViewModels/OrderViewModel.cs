using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
	public class OrderViewModel
	{
		public int Id { get; set; }
		public double Total { get; set; }
		public string PayMethod { get; set; }
		public string Status { get; set; }
		public DateTime Date { get; set; }
		public string Images { get; set; }
	}
}
