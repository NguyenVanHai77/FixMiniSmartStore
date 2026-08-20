using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSmartstoreMvc.Data;
using MiniSmartstoreMvc.Extensions;

namespace MiniSmartstoreMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .AvailableForSale(now)
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            // ===== LƯU Ý: LẤY SẢN PHẨM GIẢM GIÁ CAO NHẤT =====
            var saleProducts = await _context.Products
                .Include(p => p.Category)
                .AvailableForSale(now)
                .Where(p =>
                    p.OldPrice.HasValue &&
                    p.OldPrice.Value > 0 &&
                    p.OldPrice.Value > p.Price)
                .ToListAsync();

            var promotionProduct = saleProducts
                .OrderByDescending(p =>
                    (p.OldPrice!.Value - p.Price) / p.OldPrice.Value)
                .ThenByDescending(p => p.CreatedAt)
                .FirstOrDefault();

            ViewBag.PromotionProduct = promotionProduct;
            // ===== KẾT THÚC LẤY SẢN PHẨM GIẢM GIÁ CAO NHẤT =====

            // ===== LƯU Ý: CHỈ HIỆN POPUP 1 LẦN TRONG MỖI LẦN CHẠY DỰ ÁN =====
            var promotionShown = HttpContext.Session.GetString("HomePromotionShown");

            if (promotionProduct != null && promotionShown != "true")
            {
                ViewBag.ShowPromotionPopup = true;
                HttpContext.Session.SetString("HomePromotionShown", "true");
            }
            else
            {
                ViewBag.ShowPromotionPopup = false;
            }
            // ===== KẾT THÚC CHỈ HIỆN POPUP 1 LẦN TRONG MỖI LẦN CHẠY DỰ ÁN =====

            ViewBag.Categories = categories;

            return View(featuredProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}