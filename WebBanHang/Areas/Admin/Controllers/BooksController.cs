using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;
using WebBanHang.ViewModels;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public BooksController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        private async Task PopulateDropdownsAsync(int? selectedCategory = null, int? selectedAuthor = null, int? selectedPublisher = null)
        {
            var categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();
            var authors = await _context.Authors.OrderBy(a => a.AuthorName).ToListAsync();
            var publishers = await _context.Publishers.OrderBy(p => p.PublisherName).ToListAsync();

            ViewBag.CategoryId = new SelectList(categories, "CategoryId", "CategoryName", selectedCategory);
            ViewBag.AuthorId = new SelectList(authors, "AuthorId", "AuthorName", selectedAuthor);
            ViewBag.PublisherId = new SelectList(publishers, "PublisherId", "PublisherName", selectedPublisher);
        }

        // GET: /Admin/Books
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(b => b.BookName.Contains(term)
                    || (b.ISBN != null && b.ISBN.Contains(term))
                    || (b.Author != null && b.Author.AuthorName.Contains(term)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            var books = await query.OrderByDescending(b => b.BookId).ToListAsync();
            return View(books);
        }

        // GET: /Admin/Books/Create
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            return View(new BookViewModel());
        }

        // POST: /Admin/Books/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check unique ISBN if provided
                if (!string.IsNullOrWhiteSpace(model.ISBN))
                {
                    var exists = await _context.Books.AnyAsync(b => b.ISBN == model.ISBN.Trim());
                    if (exists)
                    {
                        ModelState.AddModelError("ISBN", "Mã ISBN này đã tồn tại trong hệ thống.");
                        await PopulateDropdownsAsync(model.CategoryId, model.AuthorId, model.PublisherId);
                        return View(model);
                    }
                }

                string? imageUrl = null;
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "images", "books");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var ext = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    imageUrl = $"/images/books/{fileName}";
                }

                var book = new Book
                {
                    BookName = model.BookName.Trim(),
                    ISBN = model.ISBN?.Trim(),
                    Price = model.Price,
                    Quantity = model.Quantity,
                    Description = model.Description?.Trim(),
                    ImageUrl = imageUrl,
                    PublishedYear = model.PublishedYear,
                    CategoryId = model.CategoryId,
                    AuthorId = model.AuthorId,
                    PublisherId = model.PublisherId,
                    CreatedAt = DateTime.Now
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Thêm mới sách '{book.BookName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(model.CategoryId, model.AuthorId, model.PublisherId);
            return View(model);
        }

        // GET: /Admin/Books/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var model = new BookViewModel
            {
                BookId = book.BookId,
                BookName = book.BookName,
                ISBN = book.ISBN,
                Price = book.Price,
                Quantity = book.Quantity,
                Description = book.Description,
                ImageUrl = book.ImageUrl,
                PublishedYear = book.PublishedYear,
                CategoryId = book.CategoryId,
                AuthorId = book.AuthorId,
                PublisherId = book.PublisherId
            };

            await PopulateDropdownsAsync(book.CategoryId, book.AuthorId, book.PublisherId);
            return View(model);
        }

        // POST: /Admin/Books/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BookViewModel model)
        {
            if (id != model.BookId) return NotFound();

            if (ModelState.IsValid)
            {
                var book = await _context.Books.FindAsync(id);
                if (book == null) return NotFound();

                // Check unique ISBN if provided and changed
                if (!string.IsNullOrWhiteSpace(model.ISBN))
                {
                    var exists = await _context.Books.AnyAsync(b => b.ISBN == model.ISBN.Trim() && b.BookId != id);
                    if (exists)
                    {
                        ModelState.AddModelError("ISBN", "Mã ISBN này đã được sử dụng cho một cuốn sách khác.");
                        await PopulateDropdownsAsync(model.CategoryId, model.AuthorId, model.PublisherId);
                        return View(model);
                    }
                }

                // Handle file upload if new image chosen
                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var uploadsDir = Path.Combine(_env.WebRootPath, "images", "books");
                    if (!Directory.Exists(uploadsDir))
                    {
                        Directory.CreateDirectory(uploadsDir);
                    }

                    var ext = Path.GetExtension(model.ImageFile.FileName).ToLowerInvariant();
                    var fileName = $"{Guid.NewGuid():N}{ext}";
                    var filePath = Path.Combine(uploadsDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }

                    book.ImageUrl = $"/images/books/{fileName}";
                }

                book.BookName = model.BookName.Trim();
                book.ISBN = model.ISBN?.Trim();
                book.Price = model.Price;
                book.Quantity = model.Quantity;
                book.Description = model.Description?.Trim();
                book.PublishedYear = model.PublishedYear;
                book.CategoryId = model.CategoryId;
                book.AuthorId = model.AuthorId;
                book.PublisherId = model.PublisherId;
                book.UpdatedAt = DateTime.Now;

                _context.Update(book);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cập nhật sách '{book.BookName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            await PopulateDropdownsAsync(model.CategoryId, model.AuthorId, model.PublisherId);
            return View(model);
        }

        // GET: /Admin/Books/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null) return NotFound();

            // Check if book is referenced in OrderDetails
            var hasOrders = await _context.OrderDetails.AnyAsync(od => od.BookId == id);
            ViewBag.HasOrders = hasOrders;

            return View(book);
        }

        // POST: /Admin/Books/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            // Verify if any order detail references this book
            var hasOrders = await _context.OrderDetails.AnyAsync(od => od.BookId == id);
            if (hasOrders)
            {
                TempData["Error"] = $"Không thể xóa sách '{book.BookName}' vì sách này đã có trong lịch sử đơn hàng của khách hàng.";
                return RedirectToAction(nameof(Index));
            }

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa sách '{book.BookName}' khỏi hệ thống.";
            return RedirectToAction(nameof(Index));
        }
    }
}
