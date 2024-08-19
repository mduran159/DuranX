using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityServer.Pages
{
    public class AuthorizeModel : PageModel
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;

        public AuthorizeModel(IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager)
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
        }

        public string ApplicationName { get; set; }
        public IEnumerable<string> Scopes { get; set; }
        public string ClientId { get; set; }
        public string ResponseType { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
                return BadRequest("Invalid request.");

            ClientId = request.ClientId;
            ResponseType = request.ResponseType;
            RedirectUri = request.RedirectUri;
            Scope = request.Scope;

            var application = await _applicationManager.FindByClientIdAsync(ClientId);
            if (application == null)
                return BadRequest("Unknown client application.");

            ApplicationName = await _applicationManager.GetDisplayNameAsync(application);
            Scopes = request.GetScopes();

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
                return BadRequest("Invalid request.");

            // Handle authorization acceptance
            await HandleAuthorizationAsync(true);

            // Redirect user back to the application
            return Redirect(request.RedirectUri);
        }

        public async Task<IActionResult> OnPostDenyAsync()
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            if (request == null)
                return BadRequest("Invalid request.");

            // Handle authorization denial
            await HandleAuthorizationAsync(false);

            // Redirect user back to the application
            return Redirect(request.RedirectUri);
        }

        private async Task HandleAuthorizationAsync(bool isAccepted)
        {
            var request = HttpContext.GetOpenIddictServerRequest();
            var user = await HttpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            var principal = user?.Principal;

            if (principal == null)
                return;

            var application = await _applicationManager.FindByClientIdAsync(request.ClientId);
            if (application == null)
                return;

            if (isAccepted)
            {
                var authorization = await _authorizationManager.CreateAsync(
                    principal: principal,
                    subject: principal.GetClaim(Claims.Subject),
                    client: await _applicationManager.GetIdAsync(application),
                    type: AuthorizationTypes.Permanent,
                    scopes: request.GetScopes());

                await _authorizationManager.UpdateAsync(authorization);
            }
            else
            {
                //await _authorizationManager.RevokeAsync(principal.GetClaim(Claims.Subject), application);
            }
        }
    }
}
