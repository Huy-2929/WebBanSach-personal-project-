using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Include(c => c.Books)
                .OrderBy(c => c.CategoryName)
                .ToListAsync();

            return View(categories);
        }

        // GET: /Admin/Categories/Create
        public IActionResult Create()
        {
            return View(new Category());
        }

        // POST: /Admin/Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category category)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == category.CategoryName.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục này đã tồn tại.");
                    return View(category);
                }

                category.CategoryName = category.CategoryName.Trim();
                category.Description = category.Description?.Trim();
                category.CreatedAt = DateTime.Now;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã thêm danh mục '{category.CategoryName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: /Admin/Categories/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories.FindAsync(id);
            if (category == null) return NotFound();

            return View(category);
        }

        // POST: /Admin/Categories/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.CategoryId) return NotFound();

            if (ModelState.IsValid)
            {
                var exists = await _context.Categories.AnyAsync(c => c.CategoryName.ToLower() == category.CategoryName.Trim().ToLower() && c.CategoryId != id);
                if (exists)
                {
                    ModelState.AddModelError("CategoryName", "Tên danh mục này đã được sử dụng cho một danh mục khác.");
                    return View(category);
                }

                var existingCategory = await _context.Categories.FindAsync(id);
                if (existingCategory == null) return NotFound();

                existingCategory.CategoryName = category.CategoryName.Trim();
                existingCategory.Description = category.Description?.Trim();

                _context.Update(existingCategory);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cập nhật danh mục '{existingCategory.CategoryName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(category);
        }

        // GET: /Admin/Categories/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(m => m.CategoryId == id);

            if (category == null) return NotFound();

            ViewBag.HasBooks = category.Books.Any();
            return View(category);
        }

        // POST: /Admin/Categories/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories
                .Include(c => c.Books)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null) return NotFound();

            if (category.Books.Any())
            {
                TempData["Error"] = $"Không thể xóa danh mục '{category.CategoryName}' vì còn sách thuộc danh mục này.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa danh mục '{category.CategoryName}' thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
