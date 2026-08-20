using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSmartstoreMvc.Data;
using MiniSmartstoreMvc.Extensions;
using MiniSmartstoreMvc.Models;
using MiniSmartstoreMvc.ViewModels;

namespace MiniSmartstoreMvc.Controllers
{
    public class CartController : Controller
    {
        private const string CartSessionKey = "GUEST_CART";

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private bool IsLoggedIn()
        {
            return User.Identity != null && User.Identity.IsAuthenticated;
        }

        private string GetUserId()
        {
            return _userManager.GetUserId(User) ?? string.Empty;
        }

        private string? NormalizeColor(string? selectedColor)
        {
            if (string.IsNullOrWhiteSpace(selectedColor))
            {
                return null;
            }

            return selectedColor.Trim();
        }

        private List<SessionCartItem> GetSessionCart()
        {
            var json = HttpContext.Session.GetString(CartSessionKey);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<SessionCartItem>();
            }

            return JsonSerializer.Deserialize<List<SessionCartItem>>(json)
                   ?? new List<SessionCartItem>();
        }

        private void SaveSessionCart(List<SessionCartItem> cart)
        {
            var json = JsonSerializer.Serialize(cart);
            HttpContext.Session.SetString(CartSessionKey, json);
        }

        private void ClearSessionCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
        }

        private async Task<string?> GetDefaultColorAsync(int productId)
        {
            var color = await _context.ProductColors
                .Where(c => c.ProductId == productId)
                .OrderBy(c => c.Id)
                .Select(c => c.ColorName)
                .FirstOrDefaultAsync();

            return NormalizeColor(color);
        }

        private async Task MergeSessionCartToUserAsync()
        {
            if (!IsLoggedIn())
            {
                return;
            }

            var sessionCart = GetSessionCart();

            if (!sessionCart.Any())
            {
                return;
            }

            var userId = GetUserId();
            var now = DateTime.Now;

            foreach (var sessionItem in sessionCart)
            {
                var selectedColor = NormalizeColor(sessionItem.SelectedColor);

                // ===== LƯU Ý: GIỮ SẢN PHẨM ĐÃ CÓ TRONG GIỎ KHI ĐỒNG BỘ SANG TÀI KHOẢN =====
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == sessionItem.ProductId);

                if (product == null)
                {
                    continue;
                }
                // ===== KẾT THÚC GIỮ SẢN PHẨM KHI ĐỒNG BỘ GIỎ =====

                if (product == null || product.StockQuantity <= 0)
                {
                    continue;
                }

                if (selectedColor == null)
                {
                    selectedColor = await GetDefaultColorAsync(product.Id);
                }

                var quantityToAdd = Math.Max(1, sessionItem.Quantity);

                var dbCartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.ProductId == sessionItem.ProductId &&
                        c.SelectedColor == selectedColor);

                if (dbCartItem == null)
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = userId,
                        ProductId = sessionItem.ProductId,
                        Quantity = quantityToAdd,
                        SelectedColor = selectedColor,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    dbCartItem.Quantity += quantityToAdd;
                    dbCartItem.UpdatedAt = now;
                }
            }

            await _context.SaveChangesAsync();
            ClearSessionCart();
        }

        private async Task<List<CartItemViewModel>> GetCartItemsAsync()
        {
            if (IsLoggedIn())
            {
                await MergeSessionCartToUserAsync();

                var userId = GetUserId();

                // ===== LƯU Ý: GIỮ SẢN PHẨM ĐÃ NGỪNG BÁN TRONG GIỎ ĐỂ KHÁCH CÓ THỂ NHẬN BIẾT VÀ XÓA =====
                var dbItems = await _context.CartItems
                    .Include(c => c.Product)
                        .ThenInclude(p => p.Category)
                    .Where(c => c.UserId == userId)
                    .ToListAsync();
                // ===== KẾT THÚC GIỮ SẢN PHẨM ĐÃ NGỪNG BÁN TRONG GIỎ =====

                return dbItems
                    .Where(c => c.Product != null)
                    .Select(c => new CartItemViewModel
                    {
                        ProductId = c.ProductId,
                        Product = c.Product,
                        Quantity = c.Quantity,
                        SelectedColor = c.SelectedColor
                    })
                    .ToList();
            }

            var sessionCart = GetSessionCart();

            var productIds = sessionCart
                .Select(c => c.ProductId)
                .Distinct()
                .ToList();

            // ===== LƯU Ý: KHÔNG LỌC THEO THỜI GIAN BÁN KHI CHỈ HIỂN THỊ GIỎ HÀNG =====
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();
            // ===== KẾT THÚC KHÔNG LỌC THEO THỜI GIAN BÁN =====

            return sessionCart
                .Select(item => new CartItemViewModel
                {
                    ProductId = item.ProductId,
                    Product = products.FirstOrDefault(p => p.Id == item.ProductId),
                    Quantity = item.Quantity,
                    SelectedColor = NormalizeColor(item.SelectedColor)
                })
                .Where(item => item.Product != null)
                .ToList();
        }

        public async Task<IActionResult> Index()
        {
            var cartItems = await GetCartItemsAsync();
            return View(cartItems);
        }

        public async Task<IActionResult> AddToCart(
            int id,
            string? selectedColor = null)
        {
            var result = await AddProductToCartAsync(id, selectedColor);

            if (!result)
            {
                return RedirectToAction(
                    "Index",
                    "Product");
            }

            TempData["Success"] = "Đã thêm sản phẩm vào giỏ hàng.";

            return RedirectToAction("Index");
        }

        private async Task<bool> AddProductToCartAsync(
            int id,
            string? selectedColor)
        {
            // ===== LƯU Ý: KIỂM TRA SẢN PHẨM CÒN TRONG THỜI GIAN BÁN =====
            var now = DateTime.Now;

            var product = await _context.Products
                .Include(p => p.ProductColors)
                .AvailableForSale(now)
                .FirstOrDefaultAsync(p => p.Id == id);
            // ===== KẾT THÚC KIỂM TRA SẢN PHẨM CÒN TRONG THỜI GIAN BÁN =====

            if (product == null)
            {
                TempData["Error"] =
                    "Sản phẩm không tồn tại hoặc đã ngừng bán.";

                return false;
            }

            if (product.StockQuantity <= 0)
            {
                TempData["Error"] = "Sản phẩm đã hết hàng.";
                return false;
            }

            selectedColor = NormalizeColor(selectedColor);

            if (selectedColor == null)
            {
                selectedColor = product.ProductColors
                    .FirstOrDefault()
                    ?.ColorName;

                selectedColor = NormalizeColor(selectedColor);
            }

            if (IsLoggedIn())
            {
                await MergeSessionCartToUserAsync();

                var userId = GetUserId();

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.ProductId == id &&
                        c.SelectedColor == selectedColor);

                if (cartItem == null)
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = userId,
                        ProductId = id,
                        Quantity = 1,
                        SelectedColor = selectedColor,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    if (cartItem.Quantity >= product.StockQuantity)
                    {
                        TempData["Error"] =
                            "Số lượng trong giỏ đã đạt tối đa tồn kho.";

                        return false;
                    }

                    cartItem.Quantity++;
                    cartItem.UpdatedAt = now;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                var sessionCart = GetSessionCart();

                var cartItem = sessionCart.FirstOrDefault(c =>
                    c.ProductId == id &&
                    NormalizeColor(c.SelectedColor) == selectedColor);

                if (cartItem == null)
                {
                    sessionCart.Add(new SessionCartItem
                    {
                        ProductId = id,
                        Quantity = 1,
                        SelectedColor = selectedColor
                    });
                }
                else
                {
                    if (cartItem.Quantity >= product.StockQuantity)
                    {
                        TempData["Error"] =
                            "Số lượng trong giỏ đã đạt tối đa tồn kho.";

                        return false;
                    }

                    cartItem.Quantity++;
                }

                SaveSessionCart(sessionCart);
            }

            return true;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartPost(
            int id,
            string? selectedColor,
            int quantity = 1)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            selectedColor = NormalizeColor(selectedColor);

            // ===== LƯU Ý: CHẶN THÊM GIỎ KHI SẢN PHẨM ĐÃ NGỪNG BÁN =====
            var now = DateTime.Now;

            var product = await _context.Products
                .Include(p => p.ProductColors)
                .AvailableForSale(now)
                .FirstOrDefaultAsync(p => p.Id == id);
            // ===== KẾT THÚC CHẶN THÊM GIỎ KHI SẢN PHẨM ĐÃ NGỪNG BÁN =====

            if (product == null)
            {
                TempData["Error"] =
                    "Sản phẩm không tồn tại hoặc đã ngừng bán.";

                return RedirectToAction(
                    "Index",
                    "Product");
            }

            if (product.StockQuantity <= 0)
            {
                TempData["Error"] = "Sản phẩm đã hết hàng.";

                return RedirectToAction(
                    "Details",
                    "Product",
                    new { id });
            }

            if (quantity > product.StockQuantity)
            {
                TempData["Error"] =
                    $"Số lượng vượt quá tồn kho. Sản phẩm chỉ còn {product.StockQuantity}.";

                return RedirectToAction(
                    "Details",
                    "Product",
                    new { id });
            }

            if (selectedColor == null)
            {
                selectedColor = product.ProductColors
                    .FirstOrDefault()
                    ?.ColorName;

                selectedColor = NormalizeColor(selectedColor);
            }

            if (IsLoggedIn())
            {
                await MergeSessionCartToUserAsync();

                var userId = GetUserId();

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.ProductId == id &&
                        c.SelectedColor == selectedColor);

                if (cartItem == null)
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = userId,
                        ProductId = id,
                        Quantity = quantity,
                        SelectedColor = selectedColor,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    cartItem.Quantity = Math.Min(
                        cartItem.Quantity + quantity,
                        product.StockQuantity);

                    cartItem.UpdatedAt = now;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                var sessionCart = GetSessionCart();

                var cartItem = sessionCart.FirstOrDefault(c =>
                    c.ProductId == id &&
                    NormalizeColor(c.SelectedColor) == selectedColor);

                if (cartItem == null)
                {
                    sessionCart.Add(new SessionCartItem
                    {
                        ProductId = id,
                        Quantity = quantity,
                        SelectedColor = selectedColor
                    });
                }
                else
                {
                    cartItem.Quantity = Math.Min(
                        cartItem.Quantity + quantity,
                        product.StockQuantity);
                }

                SaveSessionCart(sessionCart);
            }

            TempData["Success"] =
                "Đã thêm sản phẩm vào giỏ hàng.";

            var referer = Request.Headers["Referer"].ToString();

            if (!string.IsNullOrWhiteSpace(referer))
            {
                return Redirect(referer);
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(
            int productId,
            int quantity,
            string? selectedColor)
        {
            selectedColor = NormalizeColor(selectedColor);

            // ===== LƯU Ý: CHẶN CẬP NHẬT SẢN PHẨM ĐÃ NGỪNG BÁN =====
            var now = DateTime.Now;

            var product = await _context.Products
                .AvailableForSale(now)
                .FirstOrDefaultAsync(p => p.Id == productId);
            // ===== KẾT THÚC CHẶN CẬP NHẬT SẢN PHẨM ĐÃ NGỪNG BÁN =====

            if (product == null)
            {
                TempData["Error"] =
                    "Sản phẩm không tồn tại hoặc đã ngừng bán.";

                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                return await Remove(
                    productId,
                    selectedColor);
            }

            if (quantity > product.StockQuantity)
            {
                TempData["Error"] =
                    $"Số lượng vượt quá tồn kho. Sản phẩm chỉ còn {product.StockQuantity}.";

                return RedirectToAction("Index");
            }

            if (IsLoggedIn())
            {
                var userId = GetUserId();

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.ProductId == productId &&
                        c.SelectedColor == selectedColor);

                if (cartItem == null)
                {
                    return NotFound();
                }

                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = now;

                await _context.SaveChangesAsync();
            }
            else
            {
                var sessionCart = GetSessionCart();

                var cartItem = sessionCart.FirstOrDefault(c =>
                    c.ProductId == productId &&
                    NormalizeColor(c.SelectedColor) == selectedColor);

                if (cartItem == null)
                {
                    return NotFound();
                }

                cartItem.Quantity = quantity;

                SaveSessionCart(sessionCart);
            }

            TempData["Success"] =
                "Đã cập nhật giỏ hàng.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Increase(
            int productId,
            string? selectedColor)
        {
            selectedColor = NormalizeColor(selectedColor);

            var cartItems = await GetCartItemsAsync();

            var item = cartItems.FirstOrDefault(c =>
                c.ProductId == productId &&
                NormalizeColor(c.SelectedColor) == selectedColor);

            if (item == null)
            {
                return NotFound();
            }

            return await UpdateQuantity(
                productId,
                item.Quantity + 1,
                selectedColor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Decrease(
            int productId,
            string? selectedColor)
        {
            selectedColor = NormalizeColor(selectedColor);

            var cartItems = await GetCartItemsAsync();

            var item = cartItems.FirstOrDefault(c =>
                c.ProductId == productId &&
                NormalizeColor(c.SelectedColor) == selectedColor);

            if (item == null)
            {
                return NotFound();
            }

            return await UpdateQuantity(
                productId,
                item.Quantity - 1,
                selectedColor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(
            int productId,
            string? selectedColor = null)
        {
            selectedColor = NormalizeColor(selectedColor);

            if (productId <= 0)
            {
                TempData["Error"] =
                    "Sản phẩm không hợp lệ.";

                return RedirectToAction("Index");
            }

            if (IsLoggedIn())
            {
                var userId = GetUserId();

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.ProductId == productId &&
                        c.SelectedColor == selectedColor);

                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "Đã xóa sản phẩm khỏi giỏ hàng.";
                }
                else
                {
                    TempData["Error"] =
                        "Không tìm thấy sản phẩm trong giỏ hàng.";
                }
            }
            else
            {
                var sessionCart = GetSessionCart();

                var cartItem = sessionCart.FirstOrDefault(c =>
                    c.ProductId == productId &&
                    NormalizeColor(c.SelectedColor) == selectedColor);

                if (cartItem != null)
                {
                    sessionCart.Remove(cartItem);

                    SaveSessionCart(sessionCart);

                    TempData["Success"] =
                        "Đã xóa sản phẩm khỏi giỏ hàng.";
                }
                else
                {
                    TempData["Error"] =
                        "Không tìm thấy sản phẩm trong giỏ hàng.";
                }
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            if (IsLoggedIn())
            {
                var userId = GetUserId();

                var cartItems = await _context.CartItems
                    .Where(c => c.UserId == userId)
                    .ToListAsync();

                _context.CartItems.RemoveRange(cartItems);

                await _context.SaveChangesAsync();
            }
            else
            {
                ClearSessionCart();
            }

            TempData["Success"] =
                "Đã xóa toàn bộ giỏ hàng.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCartAjax(
            int id,
            int quantity = 1,
            string? selectedColor = null)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            selectedColor = NormalizeColor(selectedColor);

            // ===== LƯU Ý: CHẶN AJAX THÊM SẢN PHẨM ĐÃ NGỪNG BÁN =====
            var now = DateTime.Now;

            var product = await _context.Products
                .Include(p => p.ProductColors)
                .AvailableForSale(now)
                .FirstOrDefaultAsync(p => p.Id == id);
            // ===== KẾT THÚC CHẶN AJAX THÊM SẢN PHẨM ĐÃ NGỪNG BÁN =====

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Sản phẩm không tồn tại hoặc đã ngừng bán."
                });
            }

            if (product.StockQuantity <= 0)
            {
                return Json(new
                {
                    success = false,
                    message = "Sản phẩm đã hết hàng."
                });
            }

            if (quantity > product.StockQuantity)
            {
                return Json(new
                {
                    success = false,
                    message =
                        $"Số lượng vượt quá tồn kho. Sản phẩm chỉ còn {product.StockQuantity}."
                });
            }

            if (selectedColor == null)
            {
                selectedColor = product.ProductColors
                    .FirstOrDefault()
                    ?.ColorName;

                selectedColor = NormalizeColor(selectedColor);
            }

            if (IsLoggedIn())
            {
                await MergeSessionCartToUserAsync();

                var userId = GetUserId();

                var cartItem = await _context.CartItems
                    .FirstOrDefaultAsync(c =>
                        c.UserId == userId &&
                        c.ProductId == id &&
                        c.SelectedColor == selectedColor);

                if (cartItem == null)
                {
                    _context.CartItems.Add(new CartItem
                    {
                        UserId = userId,
                        ProductId = id,
                        Quantity = quantity,
                        SelectedColor = selectedColor,
                        CreatedAt = now,
                        UpdatedAt = now
                    });
                }
                else
                {
                    cartItem.Quantity = Math.Min(
                        cartItem.Quantity + quantity,
                        product.StockQuantity);

                    cartItem.UpdatedAt = now;
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                var sessionCart = GetSessionCart();

                var cartItem = sessionCart.FirstOrDefault(c =>
                    c.ProductId == id &&
                    NormalizeColor(c.SelectedColor) == selectedColor);

                if (cartItem == null)
                {
                    sessionCart.Add(new SessionCartItem
                    {
                        ProductId = id,
                        Quantity = quantity,
                        SelectedColor = selectedColor
                    });
                }
                else
                {
                    cartItem.Quantity = Math.Min(
                        cartItem.Quantity + quantity,
                        product.StockQuantity);
                }

                SaveSessionCart(sessionCart);
            }

            return Json(new
            {
                success = true,
                tab = "cart",
                message = $"Đã thêm {product.Name} vào giỏ hàng."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantityAjax(
            int productId,
            int quantity,
            string? selectedColor = null)
        {
            selectedColor = NormalizeColor(selectedColor);

            // ===== LƯU Ý: CHẶN AJAX CẬP NHẬT SẢN PHẨM ĐÃ NGỪNG BÁN =====
            var now = DateTime.Now;

            var product = await _context.Products
                .AvailableForSale(now)
                .FirstOrDefaultAsync(p => p.Id == productId);
            // ===== KẾT THÚC CHẶN AJAX CẬP NHẬT SẢN PHẨM ĐÃ NGỪNG BÁN =====

            if (product == null)
            {
                return Json(new
                {
                    success = false,
                    message = "Sản phẩm không tồn tại hoặc đã ngừng bán."
                });
            }

            if (quantity <= 0)
            {
                return await RemoveAjax(
                    productId,
                    selectedColor);
            }

            if (quantity > product.StockQuantity)
            {
                return Json(new
                {
                    success = false,
                    message =
                        $"Số lượng vượt quá tồn kho. Sản phẩm chỉ còn {product.StockQuantity}."
                });
            }

            if (IsLoggedIn())
            {
                var userId = GetUserId();

                var cartItemQuery = _context.CartItems
                    .Where(c =>
                        c.UserId == userId &&
                        c.ProductId == productId);

                if (selectedColor != null)
                {
                    cartItemQuery = cartItemQuery
                        .Where(c => c.SelectedColor == selectedColor);
                }

                var cartItem = await cartItemQuery
                    .FirstOrDefaultAsync();

                if (cartItem == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm trong giỏ hàng."
                    });
                }

                cartItem.Quantity = quantity;
                cartItem.UpdatedAt = now;

                await _context.SaveChangesAsync();
            }
            else
            {
                var sessionCart = GetSessionCart();

                var cartItem = sessionCart.FirstOrDefault(c =>
                    c.ProductId == productId &&
                    (selectedColor == null ||
                     NormalizeColor(c.SelectedColor) == selectedColor));

                if (cartItem == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không tìm thấy sản phẩm trong giỏ hàng."
                    });
                }

                cartItem.Quantity = quantity;

                SaveSessionCart(sessionCart);
            }

            return Json(new
            {
                success = true,
                tab = "cart",
                message = "Đã cập nhật số lượng."
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAjax(
            int productId,
            string? selectedColor = null)
        {
            selectedColor = NormalizeColor(selectedColor);

            if (IsLoggedIn())
            {
                var userId = GetUserId();

                var cartItemQuery = _context.CartItems
                    .Where(c =>
                        c.UserId == userId &&
                        c.ProductId == productId);

                if (selectedColor != null)
                {
                    cartItemQuery = cartItemQuery
                        .Where(c => c.SelectedColor == selectedColor);
                }

                var cartItem = await cartItemQuery
                    .FirstOrDefaultAsync();

                if (cartItem != null)
                {
                    _context.CartItems.Remove(cartItem);

                    await _context.SaveChangesAsync();
                }
            }
            else
            {
                var sessionCart = GetSessionCart();

                var cartItem = sessionCart.FirstOrDefault(c =>
                    c.ProductId == productId &&
                    (selectedColor == null ||
                     NormalizeColor(c.SelectedColor) == selectedColor));

                if (cartItem != null)
                {
                    sessionCart.Remove(cartItem);

                    SaveSessionCart(sessionCart);
                }
            }

            return Json(new
            {
                success = true,
                tab = "cart",
                message = "Đã xóa sản phẩm khỏi giỏ hàng."
            });
        }
    }
}