using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;

namespace WebBanHang.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Category
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Books)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Category/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Books)
                    .ThenInclude(b => b.Author)
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
}
