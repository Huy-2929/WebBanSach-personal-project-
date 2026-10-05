using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class AuthorsController : Controller
    {
        private readonly AppDbContext _context;

        public AuthorsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Authors
        public async Task<IActionResult> Index()
        {
            var authors = await _context.Authors
                .Include(a => a.Books)
                .OrderBy(a => a.AuthorName)
                .ToListAsync();

            return View(authors);
        }

        // GET: /Admin/Authors/Create
        public IActionResult Create()
        {
            return View(new Author());
        }

        // POST: /Admin/Authors/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Author author)
        {
            if (ModelState.IsValid)
            {
                author.AuthorName = author.AuthorName.Trim();
                author.Biography = author.Biography?.Trim();
                author.CreatedAt = DateTime.Now;

                _context.Authors.Add(author);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã thêm tác giả '{author.AuthorName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // GET: /Admin/Authors/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var author = await _context.Authors.FindAsync(id);
            if (author == null) return NotFound();

            return View(author);
        }

        // POST: /Admin/Authors/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Author author)
        {
            if (id != author.AuthorId) return NotFound();

            if (ModelState.IsValid)
            {
                var existingAuthor = await _context.Authors.FindAsync(id);
                if (existingAuthor == null) return NotFound();

                existingAuthor.AuthorName = author.AuthorName.Trim();
                existingAuthor.Biography = author.Biography?.Trim();

                _context.Update(existingAuthor);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cập nhật tác giả '{existingAuthor.AuthorName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(author);
        }

        // GET: /Admin/Authors/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(m => m.AuthorId == id);

            if (author == null) return NotFound();

            ViewBag.HasBooks = author.Books.Any();
            return View(author);
        }

        // POST: /Admin/Authors/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var author = await _context.Authors
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null) return NotFound();

            if (author.Books.Any())
            {
                TempData["Error"] = $"Không thể xóa tác giả '{author.AuthorName}' vì còn sách thuộc tác giả này trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa tác giả '{author.AuthorName}' thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
