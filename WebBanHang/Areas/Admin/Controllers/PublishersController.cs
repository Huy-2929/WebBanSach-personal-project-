using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class PublishersController : Controller
    {
        private readonly AppDbContext _context;

        public PublishersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Publishers
        public async Task<IActionResult> Index()
        {
            var publishers = await _context.Publishers
                .Include(p => p.Books)
                .OrderBy(p => p.PublisherName)
                .ToListAsync();

            return View(publishers);
        }

        // GET: /Admin/Publishers/Create
        public IActionResult Create()
        {
            return View(new Publisher());
        }

        // POST: /Admin/Publishers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Publisher publisher)
        {
            if (ModelState.IsValid)
            {
                var exists = await _context.Publishers.AnyAsync(p => p.PublisherName.ToLower() == publisher.PublisherName.Trim().ToLower());
                if (exists)
                {
                    ModelState.AddModelError("PublisherName", "Tên nhà xuất bản này đã tồn tại trong hệ thống.");
                    return View(publisher);
                }

                publisher.PublisherName = publisher.PublisherName.Trim();
                publisher.Address = publisher.Address?.Trim();
                publisher.Phone = publisher.Phone?.Trim();
                publisher.CreatedAt = DateTime.Now;

                _context.Publishers.Add(publisher);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã thêm nhà xuất bản '{publisher.PublisherName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(publisher);
        }

        // GET: /Admin/Publishers/Edit/{id}
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var publisher = await _context.Publishers.FindAsync(id);
            if (publisher == null) return NotFound();

            return View(publisher);
        }

        // POST: /Admin/Publishers/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Publisher publisher)
        {
            if (id != publisher.PublisherId) return NotFound();

            if (ModelState.IsValid)
            {
                var exists = await _context.Publishers.AnyAsync(p => p.PublisherName.ToLower() == publisher.PublisherName.Trim().ToLower() && p.PublisherId != id);
                if (exists)
                {
                    ModelState.AddModelError("PublisherName", "Tên nhà xuất bản này đã được sử dụng.");
                    return View(publisher);
                }

                var existing = await _context.Publishers.FindAsync(id);
                if (existing == null) return NotFound();

                existing.PublisherName = publisher.PublisherName.Trim();
                existing.Address = publisher.Address?.Trim();
                existing.Phone = publisher.Phone?.Trim();

                _context.Update(existing);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Cập nhật nhà xuất bản '{existing.PublisherName}' thành công!";
                return RedirectToAction(nameof(Index));
            }

            return View(publisher);
        }

        // GET: /Admin/Publishers/Delete/{id}
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(m => m.PublisherId == id);

            if (publisher == null) return NotFound();

            ViewBag.HasBooks = publisher.Books.Any();
            return View(publisher);
        }

        // POST: /Admin/Publishers/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var publisher = await _context.Publishers
                .Include(p => p.Books)
                .FirstOrDefaultAsync(p => p.PublisherId == id);

            if (publisher == null) return NotFound();

            if (publisher.Books.Any())
            {
                TempData["Error"] = $"Không thể xóa nhà xuất bản '{publisher.PublisherName}' vì còn sách liên kết trong hệ thống.";
                return RedirectToAction(nameof(Index));
            }

            _context.Publishers.Remove(publisher);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa nhà xuất bản '{publisher.PublisherName}' thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}
