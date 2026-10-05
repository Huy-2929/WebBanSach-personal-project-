using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Data;
using WebBanHang.Models;
using WebBanHang.ViewModels;

namespace WebBanHang.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories
                .Take(6)
                .ToListAsync();

            var featuredBooks = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .OrderByDescending(b => b.Price)
                .Take(4)
                .ToListAsync();

            var newBooks = await _context.Books
                .Include(b => b.Category)
                .Include(b => b.Author)
                .OrderByDescending(b => b.CreatedAt)
                .Take(8)
                .ToListAsync();

            var viewModel = new HomeViewModel
            {
                Categories = categories,
                FeaturedBooks = featuredBooks,
                NewBooks = newBooks
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
