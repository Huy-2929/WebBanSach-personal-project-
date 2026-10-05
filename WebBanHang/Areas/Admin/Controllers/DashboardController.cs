using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Dashboard
        public async Task<IActionResult> Index()
        {
            var totalBooks = await _context.Books.CountAsync();
            var totalCategories = await _context.Categories.CountAsync();
            var totalAuthors = await _context.Authors.CountAsync();
            var totalPublishers = await _context.Publishers.CountAsync();
            var totalUsers = await _context.Users.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();

            // Total revenue from all orders except cancelled
            var totalRevenue = await _context.Orders
                .Where(o => o.Status != "Cancelled")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var recentOrders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var viewModel = new DashboardViewModel
            {
                TotalBooks = totalBooks,
                TotalCategories = totalCategories,
                TotalAuthors = totalAuthors,
                TotalPublishers = totalPublishers,
                TotalUsers = totalUsers,
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders
            };

            return View(viewModel);
        }
    }
}
