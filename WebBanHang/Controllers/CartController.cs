using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Helpers;
using WebBanHang.ViewModels;

namespace WebBanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private List<CartItemViewModel> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
        }

        private void SaveCart(List<CartItemViewModel> cart)
        {
            HttpContext.Session.SetObject(CartSessionKey, cart);
        }

        // GET: /Cart
        public IActionResult Index()
        {
            var cart = GetCart();
            return View(cart);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var book = await _context.Books.FindAsync(bookId);
            if (book == null)
            {
                TempData["Error"] = "Sách không tồn tại trong hệ thống.";
                return RedirectToAction("Index", "Book");
            }

            if (book.Quantity <= 0)
            {
                TempData["Error"] = $"Sách '{book.BookName}' hiện đã hết hàng.";
                return RedirectToAction("Details", "Book", new { id = bookId });
            }

            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.BookId == bookId);

            if (item != null)
            {
                int newQty = item.Quantity + quantity;
                if (newQty > book.Quantity)
                {
                    item.Quantity = book.Quantity;
                    TempData["Error"] = $"Chỉ có thể mua tối đa {book.Quantity} cuốn '{book.BookName}' do giới hạn tồn kho.";
                }
                else
                {
                    item.Quantity = newQty;
                    TempData["Success"] = $"Đã cập nhật số lượng sách '{book.BookName}' trong giỏ hàng.";
                }
                item.StockQuantity = book.Quantity;
            }
            else
            {
                if (quantity > book.Quantity)
                {
                    quantity = book.Quantity;
                    TempData["Error"] = $"Chỉ có thể mua tối đa {book.Quantity} cuốn '{book.BookName}' do giới hạn tồn kho.";
                }
                else
                {
                    TempData["Success"] = $"Đã thêm '{book.BookName}' vào giỏ hàng.";
                }

                cart.Add(new CartItemViewModel
                {
                    BookId = book.BookId,
                    BookName = book.BookName,
                    ImageUrl = book.ImageUrl,
                    Price = book.Price,
                    Quantity = quantity,
                    StockQuantity = book.Quantity
                });
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int bookId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.BookId == bookId);

            if (item != null)
            {
                var book = await _context.Books.FindAsync(bookId);
                int stock = book?.Quantity ?? 0;

                if (quantity <= 0)
                {
                    cart.Remove(item);
                    TempData["Success"] = $"Đã xóa '{item.BookName}' khỏi giỏ hàng.";
                }
                else if (quantity > stock)
                {
                    item.Quantity = stock;
                    item.StockQuantity = stock;
                    TempData["Error"] = $"Số lượng yêu cầu vượt quá tồn kho. Đã điều chỉnh về tối đa {stock} cuốn.";
                }
                else
                {
                    item.Quantity = quantity;
                    item.StockQuantity = stock;
                    TempData["Success"] = $"Đã cập nhật số lượng sách '{item.BookName}'.";
                }

                SaveCart(cart);
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/RemoveFromCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int bookId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.BookId == bookId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["Success"] = $"Đã xóa '{item.BookName}' khỏi giỏ hàng.";
            }

            return RedirectToAction("Index");
        }

        // POST: /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(CartSessionKey);
            TempData["Success"] = "Đã làm trống giỏ hàng.";
            return RedirectToAction("Index");
        }
    }
}
