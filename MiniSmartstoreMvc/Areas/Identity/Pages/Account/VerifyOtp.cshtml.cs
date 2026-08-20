using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSmartstoreMvc.Services;

namespace MiniSmartstoreMvc.Areas.Identity.Pages.Account
{
    public class VerifyOtpModel : PageModel
    {
        private readonly EmailSender _emailSender;

        public VerifyOtpModel(EmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        [BindProperty]
        [Required(ErrorMessage = "Vui lòng nhập mã xác minh.")]
        [RegularExpression(@"^\d{6}$",
            ErrorMessage = "Mã xác minh phải gồm đúng 6 số.")]
        public string Otp { get; set; } = "";

        public IActionResult OnGet()
        {
            var email = HttpContext.Session.GetString(
                "ResetPasswordEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToPage("./ForgotPassword");
            }

            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var savedOtp = HttpContext.Session.GetString(
                "ResetPasswordOtp");

            var email = HttpContext.Session.GetString(
                "ResetPasswordEmail");

            var expireString = HttpContext.Session.GetString(
                "ResetPasswordOtpExpire");

            if (string.IsNullOrWhiteSpace(savedOtp) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(expireString))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Phiên xác minh đã hết hạn. Vui lòng gửi lại mã.");

                return Page();
            }

            if (!DateTime.TryParse(
                    expireString,
                    out var expireTime))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Mã xác minh không hợp lệ.");

                return Page();
            }

            if (DateTime.UtcNow > expireTime)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Mã xác minh đã hết hạn. Vui lòng gửi lại mã.");

                return Page();
            }

            if (Otp != savedOtp)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Mã xác minh không chính xác.");

                return Page();
            }

            // ===== LƯU Ý: ĐÁNH DẤU OTP ĐÃ XÁC MINH =====
            HttpContext.Session.SetString(
                "ResetPasswordOtpVerified",
                "true");
            // ===== KẾT THÚC ĐÁNH DẤU OTP ĐÃ XÁC MINH =====

            return RedirectToPage("./ResetPassword");
        }

        public async Task<IActionResult> OnPostResendAsync()
        {
            var email = HttpContext.Session.GetString(
                "ResetPasswordEmail");

            if (string.IsNullOrWhiteSpace(email))
            {
                return RedirectToPage("./ForgotPassword");
            }

            // ===== LƯU Ý: TẠO OTP MỚI =====
            var otp = Random.Shared
                .Next(100000, 1000000)
                .ToString();

            HttpContext.Session.SetString(
                "ResetPasswordOtp",
                otp);

            HttpContext.Session.SetString(
                "ResetPasswordOtpExpire",
                DateTime.UtcNow
                    .AddMinutes(5)
                    .ToString("O"));
            // ===== KẾT THÚC TẠO OTP MỚI =====

            var body = $@"
                <div style='font-family:Arial,sans-serif;
                            max-width:500px;
                            margin:auto;
                            padding:30px;'>

                    <h2 style='text-align:center;'>
                        MiniSmartStore
                    </h2>

                    <p>Mã xác minh mới của bạn là:</p>

                    <div style='font-size:36px;
                                font-weight:bold;
                                text-align:center;
                                letter-spacing:10px;
                                margin:30px 0;'>
                        {otp}
                    </div>

                    <p>
                        Mã có hiệu lực trong 5 phút.
                    </p>
                </div>";

            try
            {
                await _emailSender.SendEmailAsync(
                    email,
                    "Mã xác minh mới - MiniSmartStore",
                    body);
            }
            catch
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không thể gửi lại mã xác minh.");

                return Page();
            }

            TempData["Success"] =
                "Đã gửi lại mã xác minh.";

            return RedirectToPage();
        }
    }
}