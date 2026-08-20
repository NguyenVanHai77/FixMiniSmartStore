using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSmartstoreMvc.Data;
using MiniSmartstoreMvc.Models;
using MiniSmartstoreMvc.ViewModels;

namespace MiniSmartstoreMvc.ViewComponents
{
    public class CategoryMenuViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CategoryMenuViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var now = DateTime.Now;

            var categories = await _context.Categories
                .Include(c => c.Products)
                .Where(c => c.IsActive && c.IncludeInMenu)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync();

            var menuItems = BuildCategoryTree(categories, null, now);

            return View(menuItems);
        }

        private List<CategoryMenuItemViewModel> BuildCategoryTree(
            List<Category> categories,
            int? parentId,
            DateTime now)
        {
            return categories
                .Where(c => c.ParentCategoryId == parentId)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryMenuItemViewModel
                {
                    Id = c.Id,
                    Name = c.Name,
                    Alias = c.Alias,

                    // ===== LƯU Ý: CHỈ ĐẾM SẢN PHẨM ĐANG TRONG THỜI GIAN BÁN =====
                    ProductCount = c.Products?.Count(p =>
                        p.IsActive &&
                        (!p.AvailableStartDate.HasValue ||
                         p.AvailableStartDate.Value <= now) &&
                        (!p.AvailableEndDate.HasValue ||
                         p.AvailableEndDate.Value > now)) ?? 0,
                    // ===== KẾT THÚC CHỈ ĐẾM SẢN PHẨM ĐANG TRONG THỜI GIAN BÁN =====

                    Children = BuildCategoryTree(categories, c.Id, now)
                })
                .ToList();
        }
    }
}