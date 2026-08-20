using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSmartstoreMvc.Models;

namespace MiniSmartstoreMvc.Areas.Identity.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ResetPasswordModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
            [StringLength(
                100,
                ErrorMessage = "Mật khẩu phải có ít nhất {2} ký tự.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            public string Password { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
            [DataType(DataType.Password)]
            [Compare(
                "Password",
                ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = "";
        }

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString(
                "ResetPasswordEmail");

            var verified = HttpContext.Session.GetString(
                "ResetPasswordOtpVerified");

            if (string.IsNullOrWhiteSpace(email) ||
                verified != "true")
            {
                return RedirectToPage("./ForgotPassword");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var email = HttpContext.Session.GetString(
                "ResetPasswordEmail");

            var verified = HttpContext.Session.GetString(
                "ResetPasswordOtpVerified");

            if (string.IsNullOrWhiteSpace(email) ||
                verified != "true")
            {
                return RedirectToPage("./ForgotPassword");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không tìm thấy tài khoản.");

                return Page();
            }

            // ===== LƯU Ý: TẠO TOKEN ĐẶT LẠI MẬT KHẨU =====
            var resetToken =
                await _userManager.GeneratePasswordResetTokenAsync(user);
            // ===== KẾT THÚC TẠO TOKEN ĐẶT LẠI MẬT KHẨU =====

            var result =
                await _userManager.ResetPasswordAsync(
                    user,
                    resetToken,
                    Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }

            // ===== LƯU Ý: XÓA DỮ LIỆU OTP SAU KHI ĐỔI MẬT KHẨU =====
            HttpContext.Session.Remove("ResetPasswordOtp");
            HttpContext.Session.Remove("ResetPasswordEmail");
            HttpContext.Session.Remove("ResetPasswordOtpExpire");
            HttpContext.Session.Remove("ResetPasswordOtpVerified");
            // ===== KẾT THÚC XÓA DỮ LIỆU OTP SAU KHI ĐỔI MẬT KHẨU =====

            TempData["Success"] =
                "Đổi mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.";

            return RedirectToPage("./Login");
        }
    }
}