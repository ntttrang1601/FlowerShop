using CuaHangHoa.Models;

namespace CuaHangHoa.ViewModels
{
    public class LienHeListViewModel
    {
        public IEnumerable<LienHe> LienHes { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
}
