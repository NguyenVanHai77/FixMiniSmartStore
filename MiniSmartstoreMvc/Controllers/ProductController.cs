using MiniSmartstoreMvc.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSmartstoreMvc.Data;
using MiniSmartstoreMvc.Helpers;
using MiniSmartstoreMvc.Models;
using System.Security.Claims;

namespace MiniSmartstoreMvc.Controllers
{
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductController(
            ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<IActionResult> Index(
            string? search,
            string? globalSearch,
            int? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            string? priceRange,
            string? stockStatus,
            int? minRating,
            string? deliveryTime,
            bool? saleOnly,
            string? sortOrder,
            string? viewMode)
        {
            viewMode =
                string.IsNullOrWhiteSpace(viewMode)
                    ? "grid"
                    : viewMode;


            sortOrder =
                string.IsNullOrWhiteSpace(sortOrder)
                    ? (!string.IsNullOrWhiteSpace(globalSearch)
                        ? "relevance"
                        : "featured")
                    : sortOrder;

            // ===== LƯU Ý: THỜI ĐIỂM KIỂM TRA SẢN PHẨM ĐANG ĐƯỢC PHÉP BÁN =====
            var now = DateTime.Now;
            // ===== KẾT THÚC THỜI ĐIỂM KIỂM TRA SẢN PHẨM ĐANG ĐƯỢC PHÉP BÁN =====

            var categories =
                await _context.Categories
                    .Include(c => c.ParentCategory)
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .ThenBy(c => c.Name)
                    .ToListAsync();


            var activeProductsForCount =
                await _context.Products
                    .AvailableForSale(now)
                    .Select(p => new
                    {
                        p.Id,
                        p.CategoryId
                    })
                    .ToListAsync();


            var categoryCounts =
                categories.ToDictionary(
                    c => c.Id,
                    c =>
                    {
                        var categoryAndChildIds =
                            GetCategoryAndChildIds(
                                categories,
                                c.Id);

                        return activeProductsForCount.Count(
                            p =>
                                categoryAndChildIds
                                    .Contains(p.CategoryId));
                    });


            var query =
                _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductColors)
                    .Include(p => p.ProductReviews)
                    .AvailableForSale(now)
                    .AsQueryable();


            // THANH TÌM KIẾM LỚN
            if (!string.IsNullOrWhiteSpace(
                    globalSearch))
            {
                globalSearch =
                    globalSearch.Trim();

                query =
                    ApplySmartSearch(
                        query,
                        globalSearch,
                        categories);
            }


            // Ô TÌM KIẾM NHỎ TRONG BỘ LỌC
            if (!string.IsNullOrWhiteSpace(
                    search))
            {
                search =
                    search.Trim();

                query =
                    ApplySmartSearch(
                        query,
                        search,
                        categories);
            }


            // LỌC THEO DANH MỤC
            if (categoryId.HasValue &&
                categoryId.Value > 0)
            {
                var categoryAndChildIds =
                    GetCategoryAndChildIds(
                        categories,
                        categoryId.Value);

                query = query.Where(
                    p =>
                        categoryAndChildIds
                            .Contains(p.CategoryId));
            }


            // KHOẢNG GIÁ CỐ ĐỊNH
            if (!string.IsNullOrWhiteSpace(
                    priceRange))
            {
                switch (priceRange)
                {
                    case "under_1m":
                        minPrice = null;
                        maxPrice = 1000000;
                        break;

                    case "1m_5m":
                        minPrice = 1000000;
                        maxPrice = 5000000;
                        break;

                    case "5m_10m":
                        minPrice = 5000000;
                        maxPrice = 10000000;
                        break;

                    case "10m_20m":
                        minPrice = 10000000;
                        maxPrice = 20000000;
                        break;

                    case "over_20m":
                        minPrice = 20000000;
                        maxPrice = null;
                        break;
                }
            }


            // GIÁ TỪ
            if (minPrice.HasValue)
            {
                query =
                    query.Where(
                        p =>
                            p.Price >=
                            minPrice.Value);
            }


            // GIÁ ĐẾN
            if (maxPrice.HasValue)
            {
                query =
                    query.Where(
                        p =>
                            p.Price <=
                            maxPrice.Value);
            }


            // TÌNH TRẠNG TỒN KHO
            if (!string.IsNullOrWhiteSpace(
                    stockStatus))
            {
                if (stockStatus == "in_stock")
                {
                    query =
                        query.Where(
                            p =>
                                p.StockQuantity > 0);
                }
                else if (
                    stockStatus == "out_stock")
                {
                    query =
                        query.Where(
                            p =>
                                p.StockQuantity <= 0);
                }
            }


            // ĐÁNH GIÁ
            if (minRating.HasValue)
            {
                query =
                    query.Where(
                        p =>
                            p.ProductReviews
                                .Any(
                                    r =>
                                        r.IsApproved) &&

                            p.ProductReviews
                                .Where(
                                    r =>
                                        r.IsApproved)
                                .Average(
                                    r =>
                                        (double)r.Rating)
                            >= minRating.Value);
            }


            // THỜI GIAN GIAO HÀNG
            if (!string.IsNullOrWhiteSpace(
                    deliveryTime))
            {
                if (deliveryTime == "1_2")
                {
                    query =
                        query.Where(
                            p =>
                                p.DeliveryTime ==
                                "1 - 2 ngày");
                }
                else if (
                    deliveryTime == "3_5")
                {
                    query =
                        query.Where(
                            p =>
                                p.DeliveryTime ==
                                "3 - 5 ngày");
                }
            }


            // SẢN PHẨM ĐANG SALE
            if (saleOnly == true)
            {
                query =
                    query.Where(
                        p =>
                            p.OldPrice.HasValue &&
                            p.OldPrice.Value >
                            p.Price);
            }


            // SẮP XẾP
            query =
                sortOrder switch
                {
                    "price_asc" =>
                        query.OrderBy(
                            p => p.Price),

                    "price_desc" =>
                        query.OrderByDescending(
                            p => p.Price),

                    "newest" =>
                        query.OrderByDescending(
                            p => p.CreatedAt),

                    "name_asc" =>
                        query.OrderBy(
                            p => p.Name),

                    "name_desc" =>
                        query.OrderByDescending(
                            p => p.Name),

                    "featured" =>
                        query
                            .OrderByDescending(
                                p => p.IsFeatured)
                            .ThenByDescending(
                                p => p.CreatedAt),

                    "relevance" =>
                        query,

                    _ =>
                        query
                            .OrderByDescending(
                                p => p.IsFeatured)
                            .ThenByDescending(
                                p => p.CreatedAt)
                };


            var products =
                await query.ToListAsync();


            // THANH TÌM KIẾM LỚN:
            // XẾP SẢN PHẨM PHÙ HỢP NHẤT LÊN ĐẦU
            if (sortOrder == "relevance" &&
                !string.IsNullOrWhiteSpace(
                    globalSearch))
            {
                products =
                    products
                        .OrderByDescending(
                            p =>
                                GetGlobalSearchScore(
                                    p,
                                    globalSearch))
                        .ThenByDescending(
                            p =>
                                p.IsFeatured)
                        .ThenByDescending(
                            p =>
                                p.CreatedAt)
                        .ToList();
            }


            ViewBag.Categories =
                categories;

            ViewBag.CategoryCounts =
                categoryCounts;

            ViewBag.Search =
                search;

            ViewBag.GlobalSearch =
                globalSearch;

            ViewBag.CategoryId =
                categoryId;

            ViewBag.MinPrice =
                minPrice;

            ViewBag.MaxPrice =
                maxPrice;

            ViewBag.PriceRange =
                priceRange;

            ViewBag.StockStatus =
                stockStatus;

            ViewBag.MinRating =
                minRating;

            ViewBag.DeliveryTime =
                deliveryTime;

            ViewBag.SaleOnly =
                saleOnly;

            ViewBag.SortOrder =
                sortOrder;

            ViewBag.ViewMode =
                viewMode;


            return View(products);
        }


        private IQueryable<Product>
            ApplySmartSearch(
                IQueryable<Product> query,
                string searchText,
                List<Category> categories)
        {
            if (string.IsNullOrWhiteSpace(
                    searchText))
            {
                return query;
            }


            searchText =
                searchText.Trim();


            // Ví dụ:
            // notebook
            // smartphone
            // máy tính xách tay
            // điện thoại thông minh
            //
            // Nếu toàn bộ cụm chính xác là một
            // từ đồng nghĩa của danh mục thì
            // lấy danh mục đó luôn.
            var exactCategoryIds =
                GetExactRelatedCategoryIds(
                    searchText,
                    categories);


            if (exactCategoryIds.Count > 0)
            {
                var exactSearch =
                    searchText;

                return query.Where(
                    p =>

                        exactCategoryIds
                            .Contains(p.CategoryId) ||

                        p.Name.Contains(
                            exactSearch) ||

                        (p.ProductCode != null &&
                         p.ProductCode.Contains(
                             exactSearch)) ||

                        (p.Alias != null &&
                         p.Alias.Contains(
                             exactSearch)) ||

                        (p.SeoTitle != null &&
                         p.SeoTitle.Contains(
                             exactSearch)) ||

                        (p.SeoKeywords != null &&
                         p.SeoKeywords.Contains(
                             exactSearch)) ||

                        (p.Category != null &&
                         p.Category.Name.Contains(
                             exactSearch)) ||

                        (p.Category != null &&
                         p.Category.MetaKeywords != null &&
                         p.Category.MetaKeywords.Contains(
                             exactSearch))
                );
            }


            // Nếu không phải nguyên cụm danh mục
            // thì tách từng từ.
            //
            // Samsung S20 Ultra
            // ↓
            // Samsung
            // S20
            // Ultra
            var keywords =
                searchText.Split(
                    ' ',
                    StringSplitOptions
                        .RemoveEmptyEntries |
                    StringSplitOptions
                        .TrimEntries);


            foreach (var keyword
                     in keywords)
            {
                var currentKeyword =
                    keyword;


                var relatedCategoryIds =
                    ProductSeoSearchHelper
                        .GetRelatedCategoryIds(
                            currentKeyword,
                            categories);


                query =
                    query.Where(
                        p =>

                            p.Name.Contains(
                                currentKeyword) ||

                            (p.ProductCode != null &&
                             p.ProductCode.Contains(
                                 currentKeyword)) ||

                            (p.Alias != null &&
                             p.Alias.Contains(
                                 currentKeyword)) ||

                            (p.SeoTitle != null &&
                             p.SeoTitle.Contains(
                                 currentKeyword)) ||

                            (p.SeoKeywords != null &&
                             p.SeoKeywords.Contains(
                                 currentKeyword)) ||

                            (p.Category != null &&
                             p.Category.Name.Contains(
                                 currentKeyword)) ||

                            (p.Category != null &&
                             p.Category.MetaKeywords != null &&
                             p.Category.MetaKeywords.Contains(
                                 currentKeyword)) ||

                            relatedCategoryIds
                                .Contains(
                                    p.CategoryId)
                    );
            }


            return query;
        }


        private HashSet<int>
            GetExactRelatedCategoryIds(
                string searchText,
                List<Category> categories)
        {
            var result =
                new HashSet<int>();


            var normalizedSearch =
                ProductSeoSearchHelper
                    .NormalizeForCompare(
                        searchText);


            if (string.IsNullOrWhiteSpace(
                    normalizedSearch))
            {
                return result;
            }


            foreach (var category
                     in categories)
            {
                var categoryKeywords =
                    ProductSeoSearchHelper
                        .GetCategoryKeywords(
                            category);


                bool exactMatch =
                    categoryKeywords.Any(
                        keyword =>
                            ProductSeoSearchHelper
                                .NormalizeForCompare(
                                    keyword)
                                .Equals(
                                    normalizedSearch,
                                    StringComparison
                                        .OrdinalIgnoreCase));


                if (!exactMatch)
                {
                    continue;
                }


                var ids =
                    GetCategoryAndChildIds(
                        categories,
                        category.Id);


                foreach (var id
                         in ids)
                {
                    result.Add(id);
                }
            }


            return result;
        }


        private static int
            GetGlobalSearchScore(
                Product product,
                string search)
        {
            if (string.IsNullOrWhiteSpace(
                    search))
            {
                return 0;
            }


            var keyword =
                search.Trim();


            var productName =
                product.Name ??
                string.Empty;

            var productCode =
                product.ProductCode ??
                string.Empty;

            var alias =
                product.Alias ??
                string.Empty;

            var seoTitle =
                product.SeoTitle ??
                string.Empty;

            var seoKeywords =
                product.SeoKeywords ??
                string.Empty;

            var categoryName =
                product.Category?.Name ??
                string.Empty;

            var categoryKeywords =
                product.Category
                    ?.MetaKeywords ??
                string.Empty;


            int score = 0;


            // TÊN SẢN PHẨM CHÍNH XÁC
            if (productName.Equals(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 10000;
            }


            // TÊN BẮT ĐẦU BẰNG CỤM TÌM
            if (productName.StartsWith(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 5000;
            }


            // TÊN CHỨA NGUYÊN CỤM
            if (productName.Contains(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 3000;
            }


            // MÃ SẢN PHẨM CHÍNH XÁC
            if (!string.IsNullOrWhiteSpace(
                    productCode) &&
                productCode.Equals(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 2500;
            }


            // ALIAS CHÍNH XÁC
            if (!string.IsNullOrWhiteSpace(
                    alias) &&
                alias.Equals(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 2000;
            }


            // SEO TITLE
            if (!string.IsNullOrWhiteSpace(
                    seoTitle) &&
                seoTitle.Contains(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 1500;
            }


            // SEO KEYWORDS
            if (!string.IsNullOrWhiteSpace(
                    seoKeywords) &&
                seoKeywords.Contains(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 1200;
            }


            // DANH MỤC
            if (!string.IsNullOrWhiteSpace(
                    categoryName) &&
                categoryName.Contains(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 900;
            }


            // META KEYWORDS DANH MỤC
            if (!string.IsNullOrWhiteSpace(
                    categoryKeywords) &&
                categoryKeywords.Contains(
                    keyword,
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 700;
            }


            // TỪ ĐỒNG NGHĨA DANH MỤC
            if (ProductSeoSearchHelper
                .IsCategoryRelated(
                    keyword,
                    product.Category))
            {
                score += 800;
            }


            var words =
                keyword.Split(
                    ' ',
                    StringSplitOptions
                        .RemoveEmptyEntries |
                    StringSplitOptions
                        .TrimEntries);


            if (words.Length > 0 &&
                productName.StartsWith(
                    words[0],
                    StringComparison
                        .OrdinalIgnoreCase))
            {
                score += 1000;
            }


            foreach (var word
                     in words)
            {
                if (productName.Contains(
                        word,
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    score += 500;
                }


                if (seoTitle.Contains(
                        word,
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    score += 200;
                }


                if (seoKeywords.Contains(
                        word,
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    score += 180;
                }


                if (productCode.Contains(
                        word,
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    score += 150;
                }


                if (alias.Contains(
                        word,
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    score += 120;
                }


                if (categoryName.Contains(
                        word,
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    score += 100;
                }


                if (categoryKeywords.Contains(
                        word,
                        StringComparison
                            .OrdinalIgnoreCase))
                {
                    score += 90;
                }


                if (ProductSeoSearchHelper
                    .IsCategoryRelated(
                        word,
                        product.Category))
                {
                    score += 120;
                }
            }


            return score;
        }


        [HttpGet]
        public async Task<IActionResult> Reviews(int id)
        {
            var now = DateTime.Now;

            // ===== LƯU Ý: CHỈ CHO XEM ĐÁNH GIÁ KHI SẢN PHẨM CÒN ĐƯỢC PHÉP BÁN =====
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.User)
                .AvailableForSale(now)
                .FirstOrDefaultAsync(p => p.Id == id);
            // ===== KẾT THÚC KIỂM TRA SẢN PHẨM CÒN ĐƯỢC PHÉP BÁN =====

            if (product == null)
            {
                TempData["Error"] =
                    "Sản phẩm này đã hết thời gian bán hoặc hiện không còn khả dụng.";

                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }


        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            AddReview(
                int productId,
                int rating,
                string title,
                string comment)
        {
            var userId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);


            if (string.IsNullOrWhiteSpace(
                    userId))
            {
                return Unauthorized();
            }


            // ===== LƯU Ý: CHẶN ĐÁNH GIÁ KHI SẢN PHẨM ĐÃ NGỪNG BÁN =====
            var now = DateTime.Now;

            var product = await _context.Products
                .AvailableForSale(now)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null)
            {
                TempData["Error"] =
                    "Sản phẩm này đã hết thời gian bán hoặc hiện không còn khả dụng.";

                return RedirectToAction(nameof(Index));
            }
            // ===== KẾT THÚC CHẶN ĐÁNH GIÁ SẢN PHẨM ĐÃ NGỪNG BÁN =====


            title =
                title?.Trim() ??
                string.Empty;


            comment =
                comment?.Trim() ??
                string.Empty;


            if (rating < 1 ||
                rating > 5)
            {
                TempData["ReviewError"] =
                    "Số sao đánh giá không hợp lệ.";

                return RedirectToProductReviews(
                    productId);
            }


            if (string.IsNullOrWhiteSpace(
                    title))
            {
                TempData["ReviewError"] =
                    "Vui lòng nhập tiêu đề đánh giá.";

                return RedirectToProductReviews(
                    productId);
            }


            if (title.Length > 150)
            {
                TempData["ReviewError"] =
                    "Tiêu đề đánh giá không được vượt quá 150 ký tự.";

                return RedirectToProductReviews(
                    productId);
            }


            if (string.IsNullOrWhiteSpace(
                    comment))
            {
                TempData["ReviewError"] =
                    "Vui lòng nhập nội dung đánh giá.";

                return RedirectToProductReviews(
                    productId);
            }


            if (comment.Length > 2000)
            {
                TempData["ReviewError"] =
                    "Nội dung đánh giá không được vượt quá 2000 ký tự.";

                return RedirectToProductReviews(
                    productId);
            }


            var hasReviewed =
                await _context.ProductReviews
                    .AnyAsync(
                        r =>
                            r.ProductId ==
                            productId &&

                            r.UserId ==
                            userId);


            if (hasReviewed)
            {
                TempData["ReviewError"] =
                    "Bạn đã đánh giá sản phẩm này rồi.";

                return RedirectToProductReviews(
                    productId);
            }


            var review =
                new ProductReview
                {
                    ProductId =
                        productId,

                    UserId =
                        userId,

                    Rating =
                        rating,

                    Title =
                        title,

                    Comment =
                        comment,

                    IsApproved =
                        true,

                    CreatedAt =
                        DateTime.Now
                };


            _context.ProductReviews.Add(
                review);


            await _context.SaveChangesAsync();


            TempData["ReviewSuccess"] =
                "Cảm ơn bạn đã đánh giá sản phẩm.";


            return RedirectToProductReviews(
                productId);
        }


        private IActionResult
            RedirectToProductReviews(
                int productId)
        {
            return Redirect(
                $"{Url.Action(
                    nameof(Details),
                    new
                    {
                        id = productId
                    })}#reviewsTab");
        }


        public async Task<IActionResult> Details(int id)
        {
            var now = DateTime.Now;

            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .Include(p => p.ProductColors)
                .Include(p => p.ProductReviews)
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // ===== LƯU Ý: KIỂM TRA SẢN PHẨM CÓ ĐANG ĐƯỢC PHÉP BÁN HAY KHÔNG =====
            var isAvailable =
                product.IsActive &&
                (!product.AvailableStartDate.HasValue ||
                 product.AvailableStartDate.Value <= now) &&
                (!product.AvailableEndDate.HasValue ||
                 product.AvailableEndDate.Value > now);

            if (!isAvailable)
            {
                TempData["Error"] =
                    "Sản phẩm này đã hết thời gian bán hoặc hiện không còn khả dụng.";

                return RedirectToAction(nameof(Index));
            }
            // ===== KẾT THÚC KIỂM TRA SẢN PHẨM ĐƯỢC PHÉP BÁN =====

            var relatedCategoryIds = new List<int>
    {
        product.CategoryId
    };

            if (product.Category != null &&
                product.Category.ParentCategoryId.HasValue)
            {
                var categories = await _context.Categories
                    .Where(c => c.IsActive)
                    .ToListAsync();

                relatedCategoryIds = GetCategoryAndChildIds(
                    categories,
                    product.Category.ParentCategoryId.Value);
            }

            var relatedProducts = await _context.Products
                .Include(p => p.Category)
                .AvailableForSale(now)
                .Where(p =>
                    p.Id != product.Id &&
                    relatedCategoryIds.Contains(p.CategoryId))
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.CreatedAt)
                .Take(4)
                .ToListAsync();

            SaveRecentlyViewedProduct(product.Id);

            ViewBag.RelatedProducts = relatedProducts;

            return View(product);
        }


        private const string
            RecentlyViewedCookieName =
                "MiniRecentlyViewedProducts";


        private void
            SaveRecentlyViewedProduct(
                int productId)
        {
            var ids =
                ParseRecentlyViewedIds(
                    Request.Cookies[
                        RecentlyViewedCookieName]);


            ids.Remove(productId);

            ids.Insert(
                0,
                productId);


            ids =
                ids
                    .Take(12)
                    .ToList();


            Response.Cookies.Append(
                RecentlyViewedCookieName,
                string.Join(",", ids),
                new CookieOptions
                {
                    Expires =
                        DateTimeOffset.UtcNow
                            .AddDays(30),

                    HttpOnly =
                        true,

                    IsEssential =
                        true,

                    SameSite =
                        SameSiteMode.Lax,

                    Secure =
                        Request.IsHttps
                });
        }


        private static List<int>
            ParseRecentlyViewedIds(
                string? cookieValue)
        {
            if (string.IsNullOrWhiteSpace(
                    cookieValue))
            {
                return new List<int>();
            }


            return cookieValue
                .Split(
                    ',',
                    StringSplitOptions
                        .RemoveEmptyEntries)
                .Select(
                    x =>
                        int.TryParse(
                            x,
                            out var id)
                            ? id
                            : 0)
                .Where(
                    x => x > 0)
                .Distinct()
                .Take(12)
                .ToList();
        }


        private List<int>
            GetCategoryAndChildIds(
                List<Category> categories,
                int parentId)
        {
            var result =
                new List<int>
                {
                    parentId
                };


            var childCategories =
                categories
                    .Where(
                        c =>
                            c.ParentCategoryId ==
                            parentId)
                    .ToList();


            foreach (var child
                     in childCategories)
            {
                result.AddRange(
                    GetCategoryAndChildIds(
                        categories,
                        child.Id));
            }


            return result
                .Distinct()
                .ToList();
        }
    }
}