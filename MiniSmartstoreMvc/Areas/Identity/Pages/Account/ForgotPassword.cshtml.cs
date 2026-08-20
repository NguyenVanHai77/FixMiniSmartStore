using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiniSmartstoreMvc.Models;
using MiniSmartstoreMvc.Services;

namespace MiniSmartstoreMvc.Areas.Identity.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly EmailSender _emailSender;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            EmailSender emailSender)
        {
            _userManager = userManager;
            _emailSender = emailSender;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            public string Email { get; set; } = "";
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Không tìm thấy tài khoản với email này.");

                return Page();
            }

            // ===== LƯU Ý: TẠO OTP 6 SỐ =====
            var otp = Random.Shared
                .Next(100000, 1000000)
                .ToString();

            HttpContext.Session.SetString(
                "ResetPasswordOtp",
                otp);

            HttpContext.Session.SetString(
                "ResetPasswordEmail",
                Input.Email);

            HttpContext.Session.SetString(
                "ResetPasswordOtpExpire",
                DateTime.UtcNow
                    .AddMinutes(5)
                    .ToString("O"));

            HttpContext.Session.Remove(
                "ResetPasswordOtpVerified");
            // ===== KẾT THÚC TẠO OTP 6 SỐ =====

            // ===== LƯU Ý: NỘI DUNG EMAIL OTP =====
            var body = $@"
                <div style='
                    font-family:Arial,sans-serif;
                    max-width:520px;
                    margin:auto;
                    padding:30px;
                    border:1px solid #e5e7eb;
                    border-radius:12px;'>

                    <h2 style='
                        text-align:center;
                        margin-bottom:25px;'>
                        MiniSmartStore
                    </h2>

                    <p>
                        Bạn vừa yêu cầu đặt lại mật khẩu.
                    </p>

                    <p>
                        Mã xác minh của bạn là:
                    </p>

                    <div style='
                        font-size:36px;
                        font-weight:bold;
                        text-align:center;
                        letter-spacing:10px;
                        margin:30px 0;'>
                        {otp}
                    </div>

                    <p>
                        Mã này có hiệu lực trong
                        <strong>5 phút</strong>.
                    </p>

                    <p style='
                        color:#777;
                        font-size:14px;'>
                        Nếu bạn không yêu cầu đặt lại mật khẩu,
                        hãy bỏ qua email này.
                    </p>
                </div>";
            // ===== KẾT THÚC NỘI DUNG EMAIL OTP =====

            // ===== LƯU Ý: GỬI OTP QUA EMAIL =====
            try
            {
                await _emailSender.SendEmailAsync(
                    Input.Email,
                    "Mã xác minh đặt lại mật khẩu - MiniSmartStore",
                    body);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Lỗi gửi email: " + ex.Message);

                return Page();
            }
            // ===== KẾT THÚC GỬI OTP QUA EMAIL =====

            return RedirectToPage("./VerifyOtp");
        }
    }
}