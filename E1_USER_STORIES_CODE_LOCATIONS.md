# 📋 E1 - USER AUTHENTICATION & AUTHORIZATION (US001-US008)
## Code Location & Implementation Details

---

## 📍 **US001: Đăng ký tài khoản (Register Account)**

### 📂 **File Locations:**
```
MiniSmartstoreMvc/Areas/Identity/Pages/Account/Register.cshtml       [UI - Form HTML]
MiniSmartstoreMvc/Areas/Identity/Pages/Account/Register.cshtml.cs     [Logic - Backend]
```

### 🔍 **Key Code in Register.cshtml.cs (Lines 11-114):**

**Class:** `RegisterModel : PageModel`
- **Line 35-63:** `InputModel` class (validation attributes)
- **Line 64-114:** `OnPostAsync()` method - Handle registration

**Implementation Details:**
```csharp
// Line 11: Class definition
public class RegisterModel : PageModel

// Line 35-63: InputModel with data validation
public class InputModel
{
	[Required(ErrorMessage = "Vui lòng nhập họ tên")]
	[StringLength(100)]
	public string FullName { get; set; }

	[Required(ErrorMessage = "Vui lòng nhập email")]
	[EmailAddress]
	public string Email { get; set; }

	[Phone]
	public string? PhoneNumber { get; set; }

	[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
	[StringLength(100, MinimumLength = 6)]
	[DataType(DataType.Password)]
	public string Password { get; set; }

	[Required]
	[DataType(DataType.Password)]
	[Compare("Password")]
	public string ConfirmPassword { get; set; }
}

// Line 64-114: Registration logic
public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
{
	// 1. Check if email already exists
	var existedUser = await _userManager.FindByEmailAsync(Input.Email);
	if (existedUser != null)
	{
		ModelState.AddModelError(string.Empty, "Email này đã được sử dụng.");
		return Page();
	}

	// 2. Create new user
	var user = new ApplicationUser
	{
		UserName = Input.Email,
		Email = Input.Email,
		FullName = Input.FullName,
		PhoneNumber = Input.PhoneNumber,
		EmailConfirmed = true
	};

	// 3. Create async with password
	var result = await _userManager.CreateAsync(user, Input.Password);

	// 4. Auto-assign Customer role (US007)
	if (result.Succeeded)
	{
		if (await _roleManager.RoleExistsAsync("Customer"))
		{
			await _userManager.AddToRoleAsync(user, "Customer");
		}

		// 5. Auto sign-in after registration
		await _signInManager.SignInAsync(user, isPersistent: false);
		return LocalRedirect(returnUrl);
	}

	return Page();
}
```

### 📦 **Dependencies:**
- `UserManager<ApplicationUser>` - User management
- `SignInManager<ApplicationUser>` - Sign-in management
- `RoleManager<IdentityRole>` - Role management

### ✅ **Status:** IMPLEMENTED

---

## 📍 **US002: Đăng nhập (Login)**

### 📂 **File Locations:**
```
MiniSmartstoreMvc/Areas/Identity/Pages/Account/Login.cshtml           [UI - Form HTML]
MiniSmartstoreMvc/Areas/Identity/Pages/Account/Login.cshtml.cs        [Logic - Backend]
```

### 🔍 **Key Code in Login.cshtml.cs (Lines 12-104):**

**Class:** `LoginModel : PageModel`
- **Line 33-46:** `InputModel` class (validation)
- **Line 47-59:** `OnGetAsync()` method - Display login page
- **Line 61-104:** `OnPostAsync()` method - Handle login

**Implementation Details:**
```csharp
// Line 12: Class definition
[AllowAnonymous]
public class LoginModel : PageModel

// Line 33-46: InputModel with validation
public class InputModel
{
	[Required(ErrorMessage = "Vui lòng nhập email")]
	[EmailAddress(ErrorMessage = "Email không hợp lệ")]
	public string Email { get; set; }

	[Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
	[DataType(DataType.Password)]
	public string Password { get; set; }

	[Display(Name = "Ghi nhớ đăng nhập")]
	public bool RememberMe { get; set; }
}

// Line 61-104: Login logic
public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
{
	// 1. Validate all fields
	if (!ModelState.IsValid)
	{
		return Page();
	}

	// 2. Attempt sign in with credentials
	var result = await _signInManager.PasswordSignInAsync(
		Input.Email,           // Username
		Input.Password,        // Password
		Input.RememberMe,      // Remember me option
		lockoutOnFailure: false // Account lockout
	);

	// 3. Handle different results
	if (result.Succeeded)
	{
		_logger.LogInformation("User logged in.");
		return LocalRedirect(returnUrl);  // Return to original page
	}

	if (result.RequiresTwoFactor)
	{
		return RedirectToPage("./LoginWith2fa", ...);
	}

	if (result.IsLockedOut)
	{
		_logger.LogWarning("User account locked out.");
		return RedirectToPage("./Lockout");
	}

	// 4. Invalid credentials
	ModelState.AddModelError(string.Empty, 
		"Email hoặc mật khẩu không đúng.");

	return Page();
}
```

### 📦 **Dependencies:**
- `SignInManager<ApplicationUser>` - Sign-in management
- `ILogger<LoginModel>` - Logging

### ✅ **Status:** IMPLEMENTED

---

## 📍 **US003: Đăng xuất (Logout)**

### 📂 **File Locations:**
```
MiniSmartstoreMvc/Views/Shared/_LoginPartial.cshtml               [UI - Contains logout form]
MiniSmartstoreMvc/Areas/Identity/Pages/Account/Logout.cshtml      [Logout page - auto-generated]
MiniSmartstoreMvc/Areas/Identity/Pages/Account/Logout.cshtml.cs   [Logout logic - auto-generated]
```

### 🔍 **Logout Form in _LoginPartial.cshtml (Lines 77-85):**

```html
<!-- Line 77-85: Logout form submission -->
<form asp-area="Identity"
	  asp-page="/Account/Logout"
	  asp-route-returnUrl="@Url.Action("Index", "Home", new { area = "" })"
	  method="post">
	<button type="submit" class="dropdown-item">
		<span class="ss-menu-icon ss-icon ss-icon-logout"></span>
		<span>Đăng xuất</span>
	</button>
</form>
```

### 🔍 **Auto-generated Logout Logic (Default Identity):**

The Logout page is auto-generated by ASP.NET Core Identity and handles:
1. `SignInManager.SignOutAsync()` - Remove user session
2. Clear authentication cookies
3. Redirect to return URL

**Equivalent code:**
```csharp
[HttpPost]
public async Task<IActionResult> OnPost(string? returnUrl = null)
{
	await _signInManager.SignOutAsync();

	if (returnUrl != null)
	{
		return LocalRedirect(returnUrl);
	}

	return RedirectToPage();
}
```

### 📦 **Dependencies:**
- `SignInManager<ApplicationUser>` - Sign-out management
- ASP.NET Core Identity middleware

### ✅ **Status:** IMPLEMENTED (Auto-generated)

---

## 📍 **US004: Quên mật khẩu (Forgot Password)**

### 📂 **File Locations:**
```
MiniSmartstoreMvc/Areas/Identity/Pages/Account/ForgotPassword.cshtml       [Auto-generated UI]
MiniSmartstoreMvc/Areas/Identity/Pages/Account/ForgotPassword.cshtml.cs    [Auto-generated Logic]
MiniSmartstoreMvc/Areas/Identity/Pages/Account/ResetPassword.cshtml        [Auto-generated UI]
MiniSmartstoreMvc/Areas/Identity/Pages/Account/ResetPassword.cshtml.cs     [Auto-generated Logic]
```

### Implementation:
- **ForgotPassword:** Requests user email for password reset
- **ResetPassword:** Validates token and allows user to set new password
- Uses email service for sending reset links

### ✅ **Status:** IMPLEMENTED (Auto-generated)

---

## 📍 **US005: Cập nhật hộ sơ (Update Profile)**

### 📂 **File Locations:**
```
MiniSmartstoreMvc/Controllers/CustomerController.cs                [Logic - Line 31-89]
MiniSmartstoreMvc/Views/Customer/Info.cshtml                      [UI - Form]
MiniSmartstoreMvc/ViewModels/CustomerInfoViewModel.cs             [ViewModel - Data model]
```

### 🔍 **Key Code in CustomerController.cs:**

**Class:** `CustomerController : Controller`
- **Attribute:** `[Authorize]` - Line 12 (require user login)
- **Line 31-51:** `Info()` GET method - Display user info
- **Line 53-89:** `Info()` POST method - Update user info

**Implementation Details:**
```csharp
// Line 12: Require authorization
[Authorize]
public class CustomerController : Controller

// Line 31-51: GET - Display current info
public async Task<IActionResult> Info()
{
	var user = await _userManager.GetUserAsync(User);

	if (user == null)
	{
		return Challenge();  // Redirect to login
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

// Line 53-89: POST - Update user info
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Info(CustomerInfoViewModel model)
{
	var user = await _userManager.GetUserAsync(User);

	if (user == null)
	{
		return Challenge();
	}

	// Validate model
	if (!ModelState.IsValid)
	{
		await LoadCustomerInfoOptionsAsync(model);
		return View(model);
	}

	// Update user properties
	user.FullName = model.FullName;
	user.PhoneNumber = model.PhoneNumber;
	user.Address = model.Address;
	user.PreferredShippingMethodId = model.PreferredShippingMethodId;
	user.PreferredPaymentMethod = model.PreferredPaymentMethod;

	// Save changes
	var result = await _userManager.UpdateAsync(user);

	if (result.Succeeded)
	{
		TempData["Success"] = "Đã cập nhật thông tin tài khoản.";
		return RedirectToAction(nameof(Info));
	}

	// Handle errors
	foreach (var error in result.Errors)
	{
		ModelState.AddModelError("", error.Description);
	}

	await LoadCustomerInfoOptionsAsync(model);
	return View(model);
}
```

### 📦 **ViewModel: CustomerInfoViewModel**
```csharp
public class CustomerInfoViewModel
{
	[Required(ErrorMessage = "Vui lòng nhập họ tên")]
	[StringLength(100)]
	public string FullName { get; set; }

	[Required]
	[EmailAddress]
	public string Email { get; set; }  // Read-only

	[Phone]
	public string? PhoneNumber { get; set; }

	[StringLength(255)]
	public string? Address { get; set; }

	public int? PreferredShippingMethodId { get; set; }

	public PaymentMethod? PreferredPaymentMethod { get; set; }
}
```

### ✅ **Status:** IMPLEMENTED

---

## 📍 **US006: Đổi mật khẩu (Change Password)**

### 📂 **File Locations:**
```
MiniSmartstoreMvc/Controllers/CustomerController.cs               [Logic - Line 169-189]
MiniSmartstoreMvc/Views/Customer/ChangePassword.cshtml           [UI - Form]
MiniSmartstoreMvc/ViewModels/ChangePasswordViewModel.cs          [ViewModel - Data model]
```

### 🔍 **Key Code in CustomerController.cs (Line 169-189):**

```csharp
// Line 169: GET - Display change password form
public IActionResult ChangePassword()
{
	return View(new ChangePasswordViewModel());
}

// Line 172-189: POST - Update password
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
{
	if (!ModelState.IsValid)
	{
		return View(model);
	}

	var user = await _userManager.GetUserAsync(User);

	if (user == null)
	{
		return Challenge();  // Redirect to login
	}

	// Change password using UserManager
	var result = await _userManager.ChangePasswordAsync(
		user,
		model.OldPassword,      // Current password (validation)
		model.NewPassword       // New password
	);

	if (result.Succeeded)
	{
		TempData["Success"] = "Đã đổi mật khẩu thành công.";
		return RedirectToAction(nameof(ChangePassword));
	}

	// Handle errors
	foreach (var error in result.Errors)
	{
		ModelState.AddModelError("", error.Description);
	}

	return View(model);
}
```

### 📦 **ViewModel: ChangePasswordViewModel.cs**

```csharp
public class ChangePasswordViewModel
{
	[Required(ErrorMessage = "Vui lòng nhập mật khẩu cũ")]
	[DataType(DataType.Password)]
	public string OldPassword { get; set; }

	[Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
	[DataType(DataType.Password)]
	[StringLength(100, MinimumLength = 6, 
		ErrorMessage = "Mật khẩu mới phải có ít nhất 6 ký tự")]
	public string NewPassword { get; set; }

	[Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
	[DataType(DataType.Password)]
	[Compare("NewPassword", ErrorMessage = "Mật khẩu xác nhận không khớp")]
	public string ConfirmPassword { get; set; }
}
```

### ✅ **Status:** IMPLEMENTED

---

## 📍 **US007: Gán vai trò cho người dùng (Assign Role)**

### 📂 **File Locations:**
```
MiniSmartstoreMvc/Areas/Identity/Pages/Account/Register.cshtml.cs    [Auto-assign in Register]
MiniSmartstoreMvc/Areas/Admin/Controllers/CustomerController.cs       [Admin role management]
MiniSmartstoreMvc/Program.cs                                          [Role initialization]
```

### 🔍 **Auto-assign Role in Register.cshtml.cs (Lines 101-104):**

```csharp
// Automatically assign "Customer" role when user registers
if (await _roleManager.RoleExistsAsync("Customer"))
{
	await _userManager.AddToRoleAsync(user, "Customer");
}
```

### 🔍 **Role Setup in Program.cs:**

```csharp
// Line 17-22: Setup Identity with roles
builder.Services.AddDefaultIdentity<ApplicationUser>(options => { ... })
	.AddRoles<IdentityRole>()  // Enable roles
	.AddEntityFrameworkStores<ApplicationDbContext>();
```

### 🔍 **Admin Customer Controller for Role Management:**

**File:** `MiniSmartstoreMvc/Areas/Admin/Controllers/CustomerController.cs`
- Handles admin-side customer management including role assignment
- Line 13: `[Authorize(Roles = "Admin")]` - Admin-only access

### 📦 **Dependencies:**
- `RoleManager<IdentityRole>` - Role management
- `UserManager<ApplicationUser>` - User role assignment
- ASP.NET Core Identity

### ✅ **Status:** IMPLEMENTED

---

## 📍 **US008: Kiểm tra quyền truy cập (Check Access Permissions)**

### 📂 **File Locations:**
```
MiniSmartstoreMvc/Controllers/CustomerController.cs                    [Line 12: [Authorize]]
MiniSmartstoreMvc/Areas/Admin/Controllers/*.cs                         [All have [Authorize(Roles="Admin")]]
MiniSmartstoreMvc/Views/Shared/_LoginPartial.cshtml                   [Line 41: User.IsInRole("Admin")]
```

### 🔍 **Authorization Attributes - Usage Examples:**

**1. Customer-only pages (Any authenticated user):**
```csharp
[Authorize]
public class CustomerController : Controller
{
	// Methods accessible to any logged-in user
}
```

**2. Admin-only pages:**
```csharp
// Multiple Admin Controllers use this:
[Authorize(Roles = "Admin")]
public class DashboardController : Controller { ... }

[Authorize(Roles = "Admin")]
public class CategoryController : Controller { ... }

[Authorize(Roles = "Admin")]
public class ProductController : Controller { ... }

[Authorize(Roles = "Admin")]
public class CustomerController : Controller { ... }  // Admin version
```

**3. Role-based view rendering:**
```razor
<!-- Line 41 in _LoginPartial.cshtml -->
@if (User.IsInRole("Admin"))
{
	<li>
		<a asp-area="Admin"
		   asp-controller="Dashboard"
		   asp-action="Index">
			<span>Quản trị</span>
		</a>
	</li>
}
```

### 🔍 **Permission Check Methods:**

```csharp
// Check if user is authenticated
if (User.Identity?.IsAuthenticated == true) { ... }

// Check if user has specific role
if (User.IsInRole("Admin")) { ... }

// Check specific claim
if (User.HasClaim("permission", "delete-product")) { ... }
```

### 📦 **Authorization Components:**
- `[Authorize]` attribute - Require authentication
- `[Authorize(Roles = "RoleName")]` - Require specific role
- `User.IsInRole()` - Check role in code/views
- `User.Identity?.IsAuthenticated` - Check if logged in
- ASP.NET Core Identity middleware

### ✅ **Status:** IMPLEMENTED

---

## 📊 **SUMMARY TABLE**

| US# | Feature | File(s) | Status | Role Required |
|-----|---------|---------|--------|---------------|
| US001 | Register | `Register.cshtml.cs` | ✅ Done | None |
| US002 | Login | `Login.cshtml.cs` | ✅ Done | None |
| US003 | Logout | `_LoginPartial.cshtml` + `Logout.cshtml.cs` | ✅ Done | Customer/Admin |
| US004 | Forgot Password | `ForgotPassword.cshtml.cs` | ✅ Done | None |
| US005 | Update Profile | `CustomerController.Info()` | ✅ Done | Customer |
| US006 | Change Password | `CustomerController.ChangePassword()` | ✅ Done | Customer |
| US007 | Assign Role | `Register.cshtml.cs` | ✅ Done | Admin (manage) |
| US008 | Check Permissions | `[Authorize]` attributes | ✅ Done | Various |

---

## 🔑 **Key Classes & Models**

### ApplicationUser (MiniSmartstoreMvc/Models/ApplicationUser.cs)
```csharp
public class ApplicationUser : IdentityUser
{
	public string? FullName { get; set; }
	public string? Address { get; set; }
	public DateTime CreatedAt { get; set; }
	public int? PreferredShippingMethodId { get; set; }
	public ShippingMethod? PreferredShippingMethod { get; set; }
	public PaymentMethod? PreferredPaymentMethod { get; set; }
}
```

### ApplicationDbContext (MiniSmartstoreMvc/Data/ApplicationDbContext.cs)
```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
	// Inherits user management tables from IdentityDbContext
	// Tables: AspNetUsers, AspNetRoles, AspNetUserRoles, etc.
}
```

---

## 🔗 **Related Navigation**

- **Login Page:** `/Identity/Account/Login`
- **Register Page:** `/Identity/Account/Register`
- **Logout:** Form posts to `/Identity/Account/Logout`
- **Forgot Password:** `/Identity/Account/ForgotPassword`
- **Reset Password:** `/Identity/Account/ResetPassword`
- **Customer Info:** `/Customer/Info` (requires `[Authorize]`)
- **Change Password:** `/Customer/ChangePassword` (requires `[Authorize]`)
- **Admin Dashboard:** `/Admin/Dashboard` (requires `[Authorize(Roles="Admin")]`)

---

## 📝 **Notes**

1. **Identity Pages:** Located in `Areas/Identity/Pages/Account/` - auto-generated by ASP.NET Core Identity
2. **Customer Pages:** Located in `Views/Customer/` - custom implementation for user profile
3. **Authorization:** Uses `[Authorize]` attribute and `Roles` parameter
4. **Database:** All user data stored in `AspNetUsers` table automatically managed by EF Core Identity
5. **Default Role:** "Customer" role auto-assigned on registration (US007)

---

*Generated: Nov 2026*
*Project: MiniSmartstoreMvc - .NET 10*
*Pattern: ASP.NET Core Identity + Razor Pages*
