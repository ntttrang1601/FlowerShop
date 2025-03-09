namespace CuaHangHoa.Models
{
	public class DiaChi
	{
		public string Id { get; set; }
		public string UserId { get; set; }

		public User User { get; set; }
		public string Dchi { get; set; }
	}
}
