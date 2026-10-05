using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    public class BookController : Controller
    {
        private readonly AppDbContext _context;

        public BookController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Book
        // Supports searching, category filtering, and sorting
        public async Task<IActionResult> Index(string? search, int? categoryId, string? sortOrder)
        {
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSort = sortOrder;
            ViewBag.Categories = await _context.Categories.OrderBy(c => c.CategoryName).ToListAsync();

            var query = _context.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .AsQueryable();

            // Search filter: BookName, AuthorName, ISBN
            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(b => b.BookName.Contains(term)
                    || (b.ISBN != null && b.ISBN.Contains(term))
                    || (b.Author != null && b.Author.AuthorName.Contains(term)));
            }

            // Category filter
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(b => b.CategoryId == categoryId.Value);
            }

            // Sorting
            query = sortOrder switch
            {
                "price_asc" => query.OrderBy(b => b.Price),
                "price_desc" => query.OrderByDescending(b => b.Price),
                "name_desc" => query.OrderByDescending(b => b.BookName),
                "name_asc" => query.OrderBy(b => b.BookName),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            var books = await query.ToListAsync();
            return View(books);
        }

        // GET: /Book/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var book = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .Include(b => b.Publisher)
                .FirstOrDefaultAsync(m => m.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            // Related books in the same category
            ViewBag.RelatedBooks = await _context.Books
                .Include(b => b.Author)
                .Where(b => b.CategoryId == book.CategoryId && b.BookId != book.BookId)
                .Take(4)
                .ToListAsync();

            return View(book);
        }
    }
}
