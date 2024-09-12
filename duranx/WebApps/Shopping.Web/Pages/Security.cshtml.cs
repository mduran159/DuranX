using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace Shopping.Web.Pages
{
    [Authorize(Roles = Roles.Admin)]
    public class SecurityModel : PageModel
    {
        public record SessionData(IEnumerable<Claim> Claims, IDictionary<string, string> Metadata);

        public SessionData Session { get; set; }

        public async Task OnGetAsync()
        {
            var authenticateResult = await HttpContext.AuthenticateAsync();

            var userClaims = authenticateResult.Principal?.Claims ?? Enumerable.Empty<Claim>();
            var metadata = authenticateResult.Properties?.Items ?? new Dictionary<string, string>();

            Session = new SessionData(userClaims, metadata);
        }
    }
}
