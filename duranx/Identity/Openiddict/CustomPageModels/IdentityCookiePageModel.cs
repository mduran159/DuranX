using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OpeniddictServer.CustomPageModels
{
    public class IdentityCookiePageModel : PageModel
    {
        internal readonly SignInManager<IdentityUser> _signInManager;

        public IdentityCookiePageModel(SignInManager<IdentityUser> signInManager)
        {
            _signInManager = signInManager;
        }
    }
}
