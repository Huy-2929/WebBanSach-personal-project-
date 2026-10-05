using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class OrdersController : Controller
    {
        private readonly AppDbContext _context;
        private static readonly string[] AllowedStatuses = ["Pending", "Confirmed", "Shipping", "Completed", "Cancelled"];

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Index(string? status)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                query = query.Where(o => o.Status == status);
            }

            ViewBag.CurrentStatus = status;
            ViewBag.Statuses = AllowedStatuses;

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            return View(orders);
        }

        // GET: /Admin/Orders/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null) return NotFound();

            ViewBag.Statuses = AllowedStatuses;
            return View(order);
        }

        // POST: /Admin/Orders/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int orderId, string newStatus)
        {
            if (!AllowedStatuses.Contains(newStatus))
            {
                TempData["Error"] = "Trạng thái đơn hàng không hợp lệ.";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return NotFound();

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật trạng thái đơn hàng #{orderId} sang '{newStatus}' thành công!";
            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
