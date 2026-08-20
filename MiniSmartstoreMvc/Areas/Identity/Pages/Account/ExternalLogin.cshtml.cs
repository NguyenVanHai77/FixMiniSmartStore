using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSmartstoreMvc.Models;

namespace MiniSmartstoreMvc.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ExternalLoginModel> _logger;


        private const string ProviderSessionKey =
            "ExternalLogin.Provider";

        private const string ProviderKeySessionKey =
            "ExternalLogin.ProviderKey";

        private const string ProviderDisplayNameSessionKey =
            "ExternalLogin.ProviderDisplayName";

        private const string EmailSessionKey =
            "ExternalLogin.Email";

        private const string FullNameSessionKey =
            "ExternalLogin.FullName";


        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ILogger<ExternalLoginModel> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }


        public IActionResult OnGet()
        {
            return RedirectToPage("./Login");
        }


        // ===== LƯU Ý: BẮT ĐẦU ĐĂNG NHẬP GOOGLE =====
        public IActionResult OnPost(
            string provider,
            string? returnUrl = null)
        {
            var redirectUrl =
                Url.Page(
                    "./ExternalLogin",
                    pageHandler: "Callback",
                    values: new
                    {
                        returnUrl
                    });


            var properties =
                _signInManager
                    .ConfigureExternalAuthenticationProperties(
                        provider,
                        redirectUrl);


            return new ChallengeResult(
                provider,
                properties);
        }
        // ===== KẾT THÚC BẮT ĐẦU ĐĂNG NHẬP GOOGLE =====


        // ===== LƯU Ý: GOOGLE CALLBACK =====
        public async Task<IActionResult> OnGetCallbackAsync(
            string? returnUrl = null,
            string? remoteError = null)
        {
            returnUrl ??=
                Url.Content("~/");


            if (!string.IsNullOrWhiteSpace(remoteError))
            {
                TempData["ExternalLoginError"] =
                    "Google từ chối đăng nhập hoặc quá trình xác thực đã bị hủy.";

                return RedirectToPage("./Login");
            }


            var info =
                await _signInManager
                    .GetExternalLoginInfoAsync();


            if (info == null)
            {
                TempData["ExternalLoginError"] =
                    "Không thể lấy thông tin đăng nhập từ Google.";

                return RedirectToPage("./Login");
            }


            // Google đã từng được liên kết với một tài khoản.
            var signInResult =
                await _signInManager
                    .ExternalLoginSignInAsync(
                        info.LoginProvider,
                        info.ProviderKey,
                        isPersistent: false,
                        bypassTwoFactor: true);


            if (signInResult.Succeeded)
            {
                _logger.LogInformation(
                    "User logged in with {Provider}.",
                    info.LoginProvider);


                return LocalRedirect(
                    returnUrl);
            }


            if (signInResult.IsLockedOut)
            {
                return RedirectToPage(
                    "./Lockout");
            }


            // Google chưa liên kết → lấy email + tên.
            var email =
                info.Principal.FindFirstValue(
                    ClaimTypes.Email);


            var fullName =
                info.Principal.FindFirstValue(
                    ClaimTypes.Name);


            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["ExternalLoginError"] =
                    "Google không trả về địa chỉ email.";

                return RedirectToPage("./Login");
            }


            // Lưu tạm dữ liệu Google vào Session.
            HttpContext.Session.SetString(
                ProviderSessionKey,
                info.LoginProvider);

            HttpContext.Session.SetString(
                ProviderKeySessionKey,
                info.ProviderKey);

            HttpContext.Session.SetString(
                ProviderDisplayNameSessionKey,
                info.ProviderDisplayName
                    ?? info.LoginProvider);

            HttpContext.Session.SetString(
                EmailSessionKey,
                email);

            HttpContext.Session.SetString(
                FullNameSessionKey,
                fullName ?? string.Empty);


            // Quay lại đúng trang Login,
            // nhưng yêu cầu mở panel đăng ký.
            return RedirectToPage(
                "./Login",
                new
                {
                    externalRegister = true
                });
        }
        // ===== KẾT THÚC GOOGLE CALLBACK =====


        // ===== LƯU Ý: HOÀN TẤT ĐĂNG KÝ TÀI KHOẢN GOOGLE =====
        public async Task<IActionResult>
            OnPostConfirmRegistrationAsync(
                string fullName,
                string email)
        {
            var provider =
                HttpContext.Session.GetString(
                    ProviderSessionKey);

            var providerKey =
                HttpContext.Session.GetString(
                    ProviderKeySessionKey);

            var providerDisplayName =
                HttpContext.Session.GetString(
                    ProviderDisplayNameSessionKey);

            var googleEmail =
                HttpContext.Session.GetString(
                    EmailSessionKey);


            if (string.IsNullOrWhiteSpace(provider) ||
                string.IsNullOrWhiteSpace(providerKey) ||
                string.IsNullOrWhiteSpace(googleEmail))
            {
                TempData["ExternalLoginError"] =
                    "Phiên đăng nhập Google đã hết hạn. Vui lòng đăng nhập Google lại.";

                ClearExternalLoginSession();

                return RedirectToPage(
                    "./Login");
            }


            // Email phải đúng email mà Google đã xác thực.
            if (!string.Equals(
                    email?.Trim(),
                    googleEmail,
                    StringComparison.OrdinalIgnoreCase))
            {
                TempData["ExternalLoginError"] =
                    "Email không khớp với tài khoản Google đã xác thực.";

                return RedirectToPage(
                    "./Login",
                    new
                    {
                        externalRegister = true
                    });
            }


            fullName =
                fullName?.Trim()
                ?? string.Empty;


            if (string.IsNullOrWhiteSpace(fullName))
            {
                TempData["ExternalLoginError"] =
                    "Vui lòng nhập họ tên.";

                return RedirectToPage(
                    "./Login",
                    new
                    {
                        externalRegister = true
                    });
            }


            var existingUser =
                await _userManager
                    .FindByEmailAsync(
                        googleEmail);


            if (existingUser != null)
            {
                TempData["ExternalLoginError"] =
                    "Email này đã tồn tại trong hệ thống. Vui lòng đăng nhập bằng tài khoản hiện có.";

                ClearExternalLoginSession();

                return RedirectToPage(
                    "./Login");
            }


            var user =
                new ApplicationUser
                {
                    UserName = googleEmail,

                    Email = googleEmail,

                    FullName = fullName,

                    // Email được Google xác thực.
                    EmailConfirmed = true,

                    CreatedAt = DateTime.Now
                };


            var createResult =
                await _userManager
                    .CreateAsync(user);


            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }


                TempData["ExternalLoginError"] =
                    string.Join(
                        " ",
                        createResult.Errors
                            .Select(x => x.Description));


                return RedirectToPage(
                    "./Login",
                    new
                    {
                        externalRegister = true
                    });
            }


            var loginInfo =
                new UserLoginInfo(
                    provider,
                    providerKey,
                    providerDisplayName
                        ?? provider);


            var addLoginResult =
                await _userManager
                    .AddLoginAsync(
                        user,
                        loginInfo);


            if (!addLoginResult.Succeeded)
            {
                // Không để lại user rác
                // nếu liên kết Google thất bại.
                await _userManager
                    .DeleteAsync(user);


                TempData["ExternalLoginError"] =
                    "Không thể liên kết tài khoản với Google.";

                ClearExternalLoginSession();

                return RedirectToPage(
                    "./Login");
            }


            await _signInManager
                .SignInAsync(
                    user,
                    isPersistent: false);


            _logger.LogInformation(
                "Created a new user using {Provider}.",
                provider);


            ClearExternalLoginSession();


            // Sau khi đăng ký Google thành công
            // → vào trang chủ.
            return LocalRedirect("~/");
        }
        // ===== KẾT THÚC HOÀN TẤT ĐĂNG KÝ TÀI KHOẢN GOOGLE =====


        private void ClearExternalLoginSession()
        {
            HttpContext.Session.Remove(
                ProviderSessionKey);

            HttpContext.Session.Remove(
                ProviderKeySessionKey);

            HttpContext.Session.Remove(
                ProviderDisplayNameSessionKey);

            HttpContext.Session.Remove(
                EmailSessionKey);

            HttpContext.Session.Remove(
                FullNameSessionKey);
        }
    }
}