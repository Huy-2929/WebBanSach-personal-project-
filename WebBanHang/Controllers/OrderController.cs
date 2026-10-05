using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Helpers;
using WebBanHang.Models;
using WebBanHang.ViewModels;

namespace WebBanHang.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartSessionKey = "Cart";

        public OrderController(AppDbContext context)
        {
            _context = context;
        }

        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
        }

        private int GetCurrentUserId()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdStr, out int id) ? id : 0;
        }

        // GET: /Order/Checkout
        [HttpGet]
        public async Task<IActionResult> Checkout()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sách trước khi thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            int userId = GetCurrentUserId();
            var user = await _context.Users.FindAsync(userId);

            var model = new CheckoutViewModel
            {
                CartItems = cart,
                ShippingName = user?.FullName ?? string.Empty,
                ShippingPhone = user?.Phone ?? string.Empty,
                ShippingAddress = user?.Address ?? string.Empty
            };

            return View(model);
        }

        // POST: /Order/Checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Error"] = "Giỏ hàng đang trống!";
                return RedirectToAction("Index", "Cart");
            }

            model.CartItems = cart;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            int userId = GetCurrentUserId();
            if (userId == 0)
            {
                return RedirectToAction("Login", "Account");
            }

            // Database transaction to ensure atomicity
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Verify inventory for all items in cart
                foreach (var item in cart)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book == null)
                    {
                        ModelState.AddModelError(string.Empty, $"Sách '{item.BookName}' không còn tồn tại trong hệ thống.");
                        return View(model);
                    }

                    if (book.Quantity < item.Quantity)
                    {
                        ModelState.AddModelError(string.Empty, $"Sách '{book.BookName}' chỉ còn {book.Quantity} cuốn trong kho, không đủ số lượng bạn đặt ({item.Quantity} cuốn).");
                        return View(model);
                    }
                }

                // 2. Create Order
                decimal totalAmount = cart.Sum(i => i.Price * i.Quantity);
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    ShippingName = model.ShippingName.Trim(),
                    ShippingPhone = model.ShippingPhone.Trim(),
                    ShippingAddress = model.ShippingAddress.Trim(),
                    Note = model.Note?.Trim()
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // Save to generate OrderId

                // 3. Create OrderDetails & Deduct book inventory
                foreach (var item in cart)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book != null)
                    {
                        var orderDetail = new OrderDetail
                        {
                            OrderId = order.OrderId,
                            BookId = book.BookId,
                            Quantity = item.Quantity,
                            UnitPrice = book.Price // Snapshot unit price at purchase time
                        };

                        _context.OrderDetails.Add(orderDetail);

                        // Deduct inventory
                        book.Quantity -= item.Quantity;
                        book.UpdatedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();

                // Commit transaction
                await transaction.CommitAsync();

                // Clear cart from session
                HttpContext.Session.Remove(CartSessionKey);

                TempData["Success"] = "Đặt hàng thành công!";
                return RedirectToAction("OrderSuccess", new { id = order.OrderId });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra trong quá trình xử lý đơn hàng. Vui lòng thử lại sau: " + ex.Message);
                return View(model);
            }
        }

        // GET: /Order/OrderSuccess/{id}
        [HttpGet]
        public async Task<IActionResult> OrderSuccess(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        // GET: /Order/MyOrders
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            int userId = GetCurrentUserId();
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Order/Details/{id}
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int userId = GetCurrentUserId();
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(d => d.Book)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Security: Customer can only view their own orders. Admin can view any.
            if (order.UserId != userId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(order);
        }
    }
}
