using Microsoft.AspNetCore.Authentication;

namespace MiniSmartstoreMvc.ViewModels
{
    public class ExternalLoginViewModel
    {
        public IList<AuthenticationScheme> Providers { get; set; }
            = new List<AuthenticationScheme>();

        public string? ReturnUrl { get; set; }
    }
}