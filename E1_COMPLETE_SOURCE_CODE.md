# 📄 **E1 USER STORIES - FULL SOURCE CODE**

---

## 📝 **US001 + US007: Đăng ký tài khoản & Gán vai trò**

### 📂 File: `MiniSmartstoreMvc/Areas/Identity/Pages/Account/Register.cshtml.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSmartstoreMvc.Models;

namespace MiniSmartstoreMvc.Areas.Identity.Pages.Account
{
	[AllowAnonymous]
	public class RegisterModel : PageModel
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly ILogger<RegisterModel> _logger;

		public RegisterModel(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			RoleManager<IdentityRole> roleManager,
			ILogger<RegisterModel> logger)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_roleManager = roleManager;
			_logger = logger;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public string? ReturnUrl { get; set; }

		// ========== US001: INPUT VALIDATION ==========
		public class InputModel
		{
			[Required(ErrorMessage = "Vui lòng nhập họ tên")]
			[StringLength(100, ErrorMessage = "Họ tên không được vượt quá 100 ký tự")]
			public string FullName { get; set; } = string.Empty;

			[Required(ErrorMessage = "Vui lòng nhập email")]
			[EmailAddress(ErrorMessage = "Email không hợp lệ")]
			public string Email { get; set; } = string.Empty;

			[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
			public string? PhoneNumber { get; set; }

			[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
			[StringLength(100, ErrorMessage = "Mật khẩu phải có từ {2} đến {1} ký tự", MinimumLength = 6)]
			[DataType(DataType.Password)]
			public string Password { get; set; } = string.Empty;

			[Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
			[DataType(DataType.Password)]
			[Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp")]
			public string ConfirmPassword { get; set; } = string.Empty;
		}

		public void OnGet(string? returnUrl = null)
		{
			ReturnUrl = returnUrl ?? Url.Content("~/");
		}

		// ========== US001: REGISTRATION LOGIC ==========
		public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");
			ReturnUrl = returnUrl;

			// Step 1: Validate all input fields
			if (!ModelState.IsValid)
			{
				return Page();
			}

			// Step 2: Check if email already exists
			var existedUser = await _userManager.FindByEmailAsync(Input.Email);

			if (existedUser != null)
			{
				ModelState.AddModelError(string.Empty, "Email này đã được sử dụng.");
				return Page();
			}

			// Step 3: Create new user object
			var user = new ApplicationUser
			{
				UserName = Input.Email,
				Email = Input.Email,
				FullName = Input.FullName,
				PhoneNumber = Input.PhoneNumber,
				EmailConfirmed = true
			};

			// Step 4: Save user to database with password
			var result = await _userManager.CreateAsync(user, Input.Password);

			if (result.Succeeded)
			{
				_logger.LogInformation("Người dùng đã tạo tài khoản mới.");

				// ========== US007: AUTO-ASSIGN CUSTOMER ROLE ==========
				if (await _roleManager.RoleExistsAsync("Customer"))
				{
					await _userManager.AddToRoleAsync(user, "Customer");
				}

				// Step 5: Auto sign-in user after registration
				await _signInManager.SignInAsync(user, isPersistent: false);

				return LocalRedirect(returnUrl);
			}

			// Step 6: Handle registration errors
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError(string.Empty, error.Description);
			}

			return Page();
		}
	}
}
```

---

## 🔓 **US002: Đăng nhập (Login)**

### 📂 File: `MiniSmartstoreMvc/Areas/Identity/Pages/Account/Login.cshtml.cs`

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSmartstoreMvc.Models;

namespace MiniSmartstoreMvc.Areas.Identity.Pages.Account
{
	[AllowAnonymous]
	public class LoginModel : PageModel
	{
		private readonly SignInManager<ApplicationUser> _signInManager;
		private readonly ILogger<LoginModel> _logger;

		public LoginModel(
			SignInManager<ApplicationUser> signInManager,
			ILogger<LoginModel> logger)
		{
			_signInManager = signInManager;
			_logger = logger;
		}

		[BindProperty]
		public InputModel Input { get; set; } = new();

		public string? ReturnUrl { get; set; }

		[TempData]
		public string? ErrorMessage { get; set; }

		// ========== US002: INPUT VALIDATION ==========
		public class InputModel
		{
			[Required(ErrorMessage = "Vui lòng nhập email")]
			[EmailAddress(ErrorMessage = "Email không hợp lệ")]
			public string Email { get; set; } = string.Empty;

			[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
			[DataType(DataType.Password)]
			public string Password { get; set; } = string.Empty;

			[Display(Name = "Ghi nhớ đăng nhập")]
			public bool RememberMe { get; set; }
		}

		public async Task OnGetAsync(string? returnUrl = null)
		{
			if (!string.IsNullOrEmpty(ErrorMessage))
			{
				ModelState.AddModelError(string.Empty, ErrorMessage);
			}

			returnUrl ??= Url.Content("~/");

			// Sign out any external scheme for security
			await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

			ReturnUrl = returnUrl;
		}

		// ========== US002: LOGIN LOGIC ==========
		public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
		{
			returnUrl ??= Url.Content("~/");

			// Step 1: Validate input
			if (!ModelState.IsValid)
			{
				ReturnUrl = returnUrl;
				return Page();
			}

			// Step 2: Attempt to sign in with credentials
			var result = await _signInManager.PasswordSignInAsync(
				Input.Email,                // Email as username
				Input.Password,             // Password
				Input.RememberMe,           // Remember login
				lockoutOnFailure: false     // Account lockout (false = no lockout)
			);

			// Step 3: Handle successful login
			if (result.Succeeded)
			{
				_logger.LogInformation("User logged in.");
				return LocalRedirect(returnUrl);  // Go back to original page
			}

			// Step 4: Handle Two-Factor Authentication required
			if (result.RequiresTwoFactor)
			{
				return RedirectToPage("./LoginWith2fa", new
				{
					ReturnUrl = returnUrl,
					RememberMe = Input.RememberMe
				});
			}

			// Step 5: Handle account lockout
			if (result.IsLockedOut)
			{
				_logger.LogWarning("User account locked out.");
				return RedirectToPage("./Lockout");
			}

			// Step 6: Invalid credentials error
			ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
			ReturnUrl = returnUrl;

			return Page();
		}
	}
}
```

---

## 🚪 **US003: Đăng xuất (Logout)**

### 📂 File: `MiniSmartstoreMvc/Views/Shared/_LoginPartial.cshtml`

```html
@using Microsoft.AspNetCore.Identity
@using MiniSmartstoreMvc.Models

@inject SignInManager<ApplicationUser> SignInManager
@inject UserManager<ApplicationUser> UserManager

@if (SignInManager.IsSignedIn(User))
{
	var user = await UserManager.GetUserAsync(User);
	var email = user?.Email ?? User.Identity?.Name ?? "Tài khoản";

	<div class="dropdown ss-user-dropdown">
		<button class="ss-user-trigger"
				type="button"
				data-bs-toggle="dropdown"
				aria-expanded="false">
			<span class="ss-user-trigger-icon ss-icon ss-icon-user"></span>
			<span class="ss-user-trigger-text">@email</span>
			<span class="ss-user-trigger-arrow"></span>
		</button>

		<ul class="dropdown-menu dropdown-menu-end ss-user-menu">
			<li>
				<a class="dropdown-item"
				   asp-controller="Customer"
				   asp-action="Info">
					<span class="ss-menu-icon ss-icon ss-icon-user"></span>
					<span>Tài khoản của tôi</span>
				</a>
			</li>

			<li>
				<a class="dropdown-item"
				   asp-controller="Order"
				   asp-action="Index">
					<span class="ss-menu-icon ss-icon ss-icon-order"></span>
					<span>Đơn hàng của tôi</span>
				</a>
			</li>

			@if (User.IsInRole("Admin"))
			{
				<li>
					<a class="dropdown-item"
					   asp-area="Admin"
					   asp-controller="Dashboard"
					   asp-action="Index">
						<span class="ss-menu-icon ss-icon ss-icon-admin"></span>
						<span>Quản trị</span>
					</a>
				</li>
			}

			<li>
				<a class="dropdown-item"
				   asp-controller="Wishlist"
				   asp-action="Index">
					<span class="ss-menu-icon ss-icon ss-icon-heart"></span>
					<span>Danh sách yêu thích</span>
				</a>
			</li>

			<li>
				<a class="dropdown-item"
				   asp-controller="Cart"
				   asp-action="Index">
					<span class="ss-menu-icon ss-icon ss-icon-cart"></span>
					<span>Giỏ hàng</span>
				</a>
			</li>

			<li>
				<hr class="dropdown-divider" />
			</li>

			<!-- ========== US003: LOGOUT FORM ========== -->
			<li>
				<form asp-area="Identity"
					  asp-page="/Account/Logout"
					  asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })"
					  method="post">
					<button type="submit" class="dropdown-item">
						<span class="ss-menu-icon ss-icon ss-icon-logout"></span>
						<span>Đăng xuất</span>
					</button>
				</form>
			</li>
		</ul>
	</div>
}
else
{
	<!-- Show Login link if user not signed in -->
	<a class="ss-login-link"
	   asp-area="Identity"
	   asp-page="/Account/Login">
		<span class="ss-icon ss-icon-user"></span>
		<span>Đăng nhập</span>
	</a>
}
```

**How Logout Works:**
- Form posts to `/Identity/Account/Logout` page
- ASP.NET Core Identity auto-generated Logout page handles: `SignInManager.SignOutAsync()`
- Clears authentication cookies
- Redirects to home page

---

## 👤 **US005: Cập nhật hộ sơ (Update Profile)**

### 📂 File: `MiniSmartstoreMvc/Controllers/CustomerController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniSmartstoreMvc.Data;
using MiniSmartstoreMvc.Models;
using MiniSmartstoreMvc.ViewModels;

namespace MiniSmartstoreMvc.Controllers
{
	[Authorize]  // Require user to be logged in
	public class CustomerController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly ApplicationDbContext _context;
		private readonly RoleManager<IdentityRole> _roleManager;

		public CustomerController(
			UserManager<ApplicationUser> userManager,
			ApplicationDbContext context,
			RoleManager<IdentityRole> roleManager)
		{
			_userManager = userManager;
			_context = context;
			_roleManager = roleManager;
		}

		// ========== US005: GET - DISPLAY PROFILE ==========
		public async Task<IActionResult> Info()
		{
			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return Challenge();  // Redirect to login if not found
			}

			var model = new CustomerInfoViewModel
			{
				FullName = user.FullName ?? "",
				Email = user.Email ?? "",
				PhoneNumber = user.PhoneNumber,
				Address = user.Address,
				PreferredShippingMethodId = user.PreferredShippingMethodId,
				PreferredPaymentMethod = user.PreferredPaymentMethod
			};

			await LoadCustomerInfoOptionsAsync(model);

			return View(model);
		}

		// ========== US005: POST - UPDATE PROFILE ==========
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Info(CustomerInfoViewModel model)
		{
			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return Challenge();
			}

			model.Email = user.Email ?? "";

			// Step 1: Validate input
			if (!ModelState.IsValid)
			{
				await LoadCustomerInfoOptionsAsync(model);
				return View(model);
			}

			// Step 2: Update user properties
			user.FullName = model.FullName;
			user.PhoneNumber = model.PhoneNumber;
			user.Address = model.Address;
			user.PreferredShippingMethodId = model.PreferredShippingMethodId;
			user.PreferredPaymentMethod = model.PreferredPaymentMethod;

			// Step 3: Save changes to database
			var result = await _userManager.UpdateAsync(user);

			if (result.Succeeded)
			{
				TempData["Success"] = "Đã cập nhật thông tin tài khoản.";
				return RedirectToAction(nameof(Info));
			}

			// Step 4: Handle errors
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error.Description);
			}

			await LoadCustomerInfoOptionsAsync(model);
			return View(model);
		}

		// ========== US006: GET - DISPLAY CHANGE PASSWORD FORM ==========
		public IActionResult ChangePassword()
		{
			return View(new ChangePasswordViewModel());
		}

		// ========== US006: POST - UPDATE PASSWORD ==========
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
		{
			// Step 1: Validate input
			if (!ModelState.IsValid)
			{
				return View(model);
			}

			var user = await _userManager.GetUserAsync(User);

			if (user == null)
			{
				return Challenge();  // Redirect to login
			}

			// Step 2: Change password with old password verification
			var result = await _userManager.ChangePasswordAsync(
				user,
				model.OldPassword,      // Current password (must be correct)
				model.NewPassword       // New password
			);

			// Step 3: Handle success
			if (result.Succeeded)
			{
				TempData["Success"] = "Đã đổi mật khẩu thành công.";
				return RedirectToAction(nameof(ChangePassword));
			}

			// Step 4: Handle errors
			foreach (var error in result.Errors)
			{
				ModelState.AddModelError("", error.Description);
			}

			return View(model);
		}

		// Helper method to load shipping methods and payment methods
		private async Task LoadCustomerInfoOptionsAsync(CustomerInfoViewModel model)
		{
			model.ShippingMethods = await _context.ShippingMethods
				.Where(x => x.IsActive)
				.OrderBy(x => x.Name)
				.Select(x => new SelectListItem
				{
					Value = x.Id.ToString(),
					Text = x.Name,
					Selected = model.PreferredShippingMethodId == x.Id
				})
				.ToListAsync();
		}
	}
}
```

---

## 📋 **ViewModels & Models**

### 📂 File: `MiniSmartstoreMvc/Models/ApplicationUser.cs`

```csharp
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MiniSmartstoreMvc.Models
{
	public class ApplicationUser : IdentityUser
	{
		[StringLength(100)]
		public string? FullName { get; set; }  // US001, US005

		[StringLength(255)]
		public string? Address { get; set; }   // US005

		public DateTime CreatedAt { get; set; } = DateTime.Now;

		public int? PreferredShippingMethodId { get; set; }  // US005

		public ShippingMethod? PreferredShippingMethod { get; set; }

		public PaymentMethod? PreferredPaymentMethod { get; set; }
	}
}
```

---

### 📂 File: `MiniSmartstoreMvc/ViewModels/CustomerInfoViewModel.cs`

```csharp
using Microsoft.AspNetCore.Mvc.Rendering;
using MiniSmartstoreMvc.Models;
using System.ComponentModel.DataAnnotations;

namespace MiniSmartstoreMvc.ViewModels
{
	public class CustomerInfoViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập họ tên")]
		[StringLength(100)]
		public string FullName { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;  // Read-only

		[Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
		public string? PhoneNumber { get; set; }

		[StringLength(255)]
		public string? Address { get; set; }

		public bool SubscribeNewsletter { get; set; }

		public int? PreferredShippingMethodId { get; set; }

		public PaymentMethod? PreferredPaymentMethod { get; set; }

		// For rendering dropdowns in form
		public List<SelectListItem> ShippingMethods { get; set; } = new();

		public List<SelectListItem> PaymentMethods { get; set; } = new();
	}
}
```

---

### 📂 File: `MiniSmartstoreMvc/ViewModels/ChangePasswordViewModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace MiniSmartstoreMvc.ViewModels
{
	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
		[DataType(DataType.Password)]
		public string OldPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
		[DataType(DataType.Password)]
		[StringLength(100, MinimumLength = 6, 
			ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
		public string NewPassword { get; set; } = string.Empty;

		[Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
		[DataType(DataType.Password)]
		[Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
		public string ConfirmPassword { get; set; } = string.Empty;
	}
}
```

---

## 🔐 **US008: Authorization Attributes (Check Permissions)**

### 📂 Examples from Controllers:

#### Customer-only (Any logged-in user):
```csharp
[Authorize]  // Line 12 in CustomerController.cs
public class CustomerController : Controller
{
	// All methods accessible only to logged-in users
}
```

#### Admin-only (Admin role required):
```csharp
// MiniSmartstoreMvc/Areas/Admin/Controllers/DashboardController.cs
[Authorize(Roles = "Admin")]
public class DashboardController : Controller { ... }

// MiniSmartstoreMvc/Areas/Admin/Controllers/CategoryController.cs
[Authorize(Roles = "Admin")]
public class CategoryController : Controller { ... }

// MiniSmartstoreMvc/Areas/Admin/Controllers/ProductController.cs
[Authorize(Roles = "Admin")]
public class ProductController : Controller { ... }

// MiniSmartstoreMvc/Areas/Admin/Controllers/CustomerController.cs
[Authorize(Roles = "Admin")]
public class CustomerController : Controller { ... }
```

#### Role check in Views:
```razor
<!-- Line 41-52 in _LoginPartial.cshtml -->
@if (User.IsInRole("Admin"))
{
	<li>
		<a class="dropdown-item"
		   asp-area="Admin"
		   asp-controller="Dashboard"
		   asp-action="Index">
			<span>Quản trị</span>
		</a>
	</li>
}
```

---

## 🔑 **Key Points Summary**

| Feature | Class/Method | Line |
|---------|--------------|------|
| **US001** Register | RegisterModel.OnPostAsync() | 64-114 |
| **US002** Login | LoginModel.OnPostAsync() | 61-104 |
| **US003** Logout | Form posts to /Identity/Account/Logout | _LoginPartial 77-85 |
| **US005** Update Profile | CustomerController.Info() | 29-93 |
| **US006** Change Password | CustomerController.ChangePassword() | 169-208 |
| **US007** Assign Role | RegisterModel.OnPostAsync() | 97-100 |
| **US008** Authorization | [Authorize] attributes | Various |

---

*Generated: November 2026*
*Project: MiniSmartstoreMvc - .NET 10*
