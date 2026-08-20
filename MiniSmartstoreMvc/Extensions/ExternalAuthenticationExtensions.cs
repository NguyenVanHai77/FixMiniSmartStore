using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MiniSmartstoreMvc.Extensions
{
    public static class ExternalAuthenticationExtensions
    {
        public static IServiceCollection AddExternalLoginProviders(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var authentication =
                services.AddAuthentication();


            // ===== LƯU Ý: GOOGLE AUTHENTICATION =====
            AddGoogleAuthentication(
                authentication,
                configuration);
            // ===== KẾT THÚC GOOGLE AUTHENTICATION =====


            /*
             * Sau này có thể thêm:
             *
             * AddMicrosoftAuthentication(
             *     authentication,
             *     configuration);
             *
             * AddFacebookAuthentication(
             *     authentication,
             *     configuration);
             */


            return services;
        }


        private static void AddGoogleAuthentication(
            AuthenticationBuilder authentication,
            IConfiguration configuration)
        {
            var clientId =
                configuration[
                    "Authentication:Google:ClientId"
                ];


            var clientSecret =
                configuration[
                    "Authentication:Google:ClientSecret"
                ];


            /*
             * Nếu máy hiện tại chưa cấu hình Google,
             * không đăng ký provider.
             *
             * Khi đó website vẫn chạy bình thường,
             * chỉ không hiện nút Google.
             */
            if (string.IsNullOrWhiteSpace(clientId) ||
                string.IsNullOrWhiteSpace(clientSecret))
            {
                return;
            }


            authentication.AddGoogle(
                "Google",
                options =>
                {
                    options.ClientId =
                        clientId;

                    options.ClientSecret =
                        clientSecret;


                    /*
                     * Không hard-code:
                     *
                     * https://localhost:7005/signin-google
                     *
                     * Chỉ sử dụng path.
                     *
                     * Host hiện tại sẽ được ASP.NET Core
                     * lấy từ request.
                     */
                    options.CallbackPath =
                        "/signin-google";


                    options.SaveTokens =
                        true;
                });
        }
    }
}