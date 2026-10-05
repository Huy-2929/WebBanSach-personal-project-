using WebBanHang.Models;

namespace WebBanHang.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalCategories { get; set; }
        public int TotalAuthors { get; set; }
        public int TotalPublishers { get; set; }
        public int TotalUsers { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }

        public List<Order> RecentOrders { get; set; } = new List<Order>();
    }
}
