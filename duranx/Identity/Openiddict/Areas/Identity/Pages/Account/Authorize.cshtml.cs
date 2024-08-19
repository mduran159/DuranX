using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Collections.Immutable;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using OpeniddictServer.CustomPageModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using OpenIddict.Server;

namespace OpeniddictServer.Areas.Identity.Pages.Account
{
    [ValidateAntiForgeryToken]
    public class AuthorizeModel : IdentityCookiePageModel
    {
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IOptions<OpenIddictServerOptions> _options;

        public AuthorizeModel(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager, IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager, IOptions<OpenIddictServerOptions> options)
        : base(signInManager)
        {
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _userManager = userManager;
            _options = options;
        }

        [BindProperty]
        public string ApplicationName { get; set; }

        [BindProperty]
        public string Scopes { get; set; }

        [BindProperty]
        public string ClientId { get; set; }

        [BindProperty]
        public string ResponseType { get; set; }

        [BindProperty]
        public string State { get; set; }

        [BindProperty]
        public string RedirectUri { get; set; }

        public string ReturnUrlQueryString { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ReturnUrlQueryString = HttpContext.Request.QueryString.Value!;
            ClientId = HtmlEncoder.Default.Encode(HttpContext.Request.Query["client_id"].FirstOrDefault()!);
            ResponseType = HtmlEncoder.Default.Encode(HttpContext.Request.Query["response_type"].FirstOrDefault()!);
            RedirectUri = HtmlEncoder.Default.Encode(HttpContext.Request.Query["redirect_uri"].FirstOrDefault()!);
            Scopes = HtmlEncoder.Default.Encode(HttpContext.Request.Query["scope"].FirstOrDefault()!);
            State = HtmlEncoder.Default.Encode(HttpContext.Request.Query["state"].FirstOrDefault()!);

            var application = await _applicationManager.FindByClientIdAsync(ClientId);
            if (application == null)
                return BadRequest("Unknown client application.");

            ApplicationName = (await _applicationManager.GetDisplayNameAsync(application))!;

            return Page();
        }

        public async Task<IActionResult> OnPostAcceptAsync(string returnUrlQueryString)
        {
            // Handle authorization acceptance
            await HandleAuthorizationAsync(true);

            // Redirect user back to the application
            return Redirect($"{_options.Value.AuthorizationEndpointUris.FirstOrDefault()!.OriginalString}{returnUrlQueryString}");
        }

        public async Task<IActionResult> OnPostDenyAsync(string returnUrlQueryString)
        {
            // Handle authorization denial
            await HandleAuthorizationAsync(false);

            // Redirect user back to the application
            return Redirect($"/Login?ReturnUrl={Uri.EscapeDataString($"{_options.Value.AuthorizationEndpointUris.FirstOrDefault()!.OriginalString}{returnUrlQueryString}")}");
        }

        private async Task HandleAuthorizationAsync(bool isAccepted)
        {
            if (!ModelState.IsValid)
            {
                return;
            }

            if (User == null)
                return;

            var application = await _applicationManager.FindByClientIdAsync(ClientId);
            if (application == null)
                return;

            if (isAccepted)
            {
                var authResult = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
                if (!authResult.Succeeded)
                    throw new InvalidOperationException("Error authenticating the user");

                var user = (await _userManager.GetUserAsync(authResult.Principal!))!;
                var principal = await _signInManager.CreateUserPrincipalAsync(user);

                var scopes = Scopes.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

                var authorization = await _authorizationManager.CreateAsync(
                    principal: principal,
                    subject: user.Id,
                    client: (await _applicationManager.GetIdAsync(application))!,
                    type: AuthorizationTypes.Permanent,
                    scopes: scopes.ToImmutableArray());

                if (authorization is null)
                    throw new InvalidOperationException("Authorization could not be saved");
                else return;
            }
            else
            {
                //await _authorizationManager.RevokeAsync(principal.GetClaim(Claims.Subject), application);
            }
        }
    }
}
