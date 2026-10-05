using WebBanHang.Models;

namespace WebBanHang.ViewModels
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Book> FeaturedBooks { get; set; } = new List<Book>();
        public List<Book> NewBooks { get; set; } = new List<Book>();
    }
}
