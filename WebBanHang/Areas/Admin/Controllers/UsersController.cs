using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly AppDbContext _context;

        public UsersController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users
                .Include(u => u.Orders)
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return View(users);
        }

        // GET: /Admin/Users/EditRole/{id}
        public async Task<IActionResult> EditRole(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: /Admin/Users/EditRole/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(int id, string role)
        {
            if (role != "Customer" && role != "Admin")
            {
                TempData["Error"] = "Vai trò không hợp lệ. Chỉ chấp nhận Customer hoặc Admin.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(currentUserIdStr, out int currentUserId) && currentUserId == id && role != "Admin")
            {
                TempData["Error"] = "Bạn không thể tự hạ quyền Admin của chính tài khoản đang đăng nhập!";
                return RedirectToAction(nameof(Index));
            }

            user.Role = role;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật vai trò cho người dùng '{user.FullName}' thành '{role}'!";
            return RedirectToAction(nameof(Index));
        }
    }
}
