using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;
using OpenIddict.Abstractions;
using OpeniddictServer.Extensions;
using System.Security.Claims;
using System.Collections.Immutable;

namespace OpeniddictServer.Handlers
{
    public class AuthorizationRequestHandler : IOpenIddictServerHandler<HandleAuthorizationRequestContext>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public AuthorizationRequestHandler(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOpenIddictApplicationManager applicationManager, IOpenIddictAuthorizationManager authorizationManager, IOpenIddictScopeManager scopeManager) 
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _scopeManager = scopeManager;
        }

        public async ValueTask HandleAsync(HandleAuthorizationRequestContext context)
        {
            var httpRequest = context.Transaction.GetHttpRequest() ??
                throw new InvalidOperationException("HTTP request cannot be retrieved.");

            var httpContext = httpRequest.HttpContext ??
                throw new InvalidOperationException("HTTP context cannot be retrieved.");

            var request = httpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("OpenID Connect request cannot be retrieved.");

            // Retrieve the user principal stored in the authentication cookie.
            // And retrieve the IdentityUser if authentication was succeeded
            // If a max_age parameter was provided, ensure that the cookie is not too old.
            // If the user principal can't be extracted or the cookie is too old, redirect the user to the login page.
            var authResult = await httpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);
            var user = authResult.Succeeded ? await _userManager.GetUserAsync(authResult.Principal) : null;
            if (!authResult.Succeeded || user is null || request.HasPrompt(Prompts.Login) ||
                        (request.MaxAge != null && authResult.Properties?.IssuedUtc != null &&
                        DateTimeOffset.UtcNow - authResult.Properties.IssuedUtc > TimeSpan.FromSeconds(request.MaxAge.Value)))
            {
                // Redirect to login
                await httpContext.ChallengeAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new AuthenticationProperties
                    {
                        RedirectUri = httpRequest.PathBase + httpRequest.Path + QueryString.Create(
                            httpRequest.HasFormContentType ? httpRequest.Form.ToList() : httpRequest.Query.ToList())
                    });

                // Manejar la respuesta después del challenge
                context.HandleRequest();
                return;
            }
            else
            {
                // Retrieve the application details from the database.
                var application = await _applicationManager.FindByClientIdAsync(request.ClientId!)
                                  ?? throw new InvalidOperationException("Details concerning the calling client application cannot be found.");

                // Retrieve the permanent authorizations associated with the user and the calling client application.
                var subject = await _userManager.GetUserIdAsync(user);
                var authorizations = await _authorizationManager.FindAsync(
                    subject: subject,
                    client: (await _applicationManager.GetIdAsync(application))!,
                    status: Statuses.Valid,
                    type: AuthorizationTypes.Permanent,
                    scopes: request.GetScopes()).ToListAsync();

                switch (await _applicationManager.GetConsentTypeAsync(application))
                {
                    // If the consent is external (e.g when authorizations are granted by a sysadmin),
                    // immediately return an error if no authorization can be found in the database.
                    case ConsentTypes.External when !authorizations.Any():
                        await httpContext.ForbidAsync(
                                OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                                new AuthenticationProperties(new Dictionary<string, string?>
                                {
                                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                    "The logged in user is not allowed to access this client application."
                                }
                            ));
                        context.HandleRequest();
                        return;

                    // If the consent is implicit or if an authorization was found,
                    // return an authorization response without displaying the consent form.
                    case ConsentTypes.Implicit:
                    case ConsentTypes.External when authorizations.Any():
                    case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
                        var principal = authResult.Principal;

                        // Note: in this sample, the granted scopes match the requested scope
                        // but you may want to allow the user to uncheck specific scopes.
                        // For that, simply restrict the list of scopes before calling SetScopes.
                        //principal.SetClaim(Scopes,string.Join(" ", request.GetScopes()));
                        //principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());
                        var permission = await _applicationManager.GetPermissionsAsync(application!);
                        var scopes = permission.Where(item => item.StartsWith("scp:"))
                                                .Select(item => item.Substring(4)) // Remove "scp:" prefix
                                                .ToImmutableArray();
                        var resources = await _scopeManager.ListResourcesAsync(scopes).ToListAsync();

                        principal.SetScopes(scopes);
                        principal.SetResources(resources);

                        // Automatically create a permanent authorization to avoid requiring explicit consent
                        // for future authorization or token requests containing the same scopes.
                        var authorization = authorizations.LastOrDefault() ??
                                            await _authorizationManager.CreateAsync(
                                                principal: principal,
                                                subject: user.Id,
                                                client: (await _applicationManager.GetIdAsync(application))!,
                                                type: AuthorizationTypes.Permanent,
                                                scopes: principal.GetScopes());

                        var authorizationId = await _authorizationManager.GetIdAsync(authorization);
                        principal.SetAuthorizationId(authorizationId);

                        if (string.IsNullOrEmpty(principal.FindFirstValue(Claims.Subject)))
                        {
                            principal.SetClaim(Claims.Subject, user.Id);
                        }

                        //principal.RemoveClaims(ClaimTypes.Email);

                        foreach (var claim in principal.Claims)
                        {
                            claim.SetDestinations(OpeniddictExtensions.GetDestinations(claim));
                        }

                        await httpContext.SignInAsync(
                            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, 
                            principal);
                        context.HandleRequest();
                        return;

                    // At this point, no authorization was found in the database and an error must be returned
                    // if the client application specified prompt=none in the authorization request.
                    case ConsentTypes.Explicit when request.HasPrompt(Prompts.None):
                    case ConsentTypes.Systematic when request.HasPrompt(Prompts.None):
                        await httpContext.ForbidAsync(
                            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                            properties: new AuthenticationProperties(new Dictionary<string, string?>
                            {
                                [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.ConsentRequired,
                                [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                    "Interactive user consent is required."
                            }));
                        context.HandleRequest();
                        return;

                    // In every other case, render the consent form.
                    default:
                        httpContext.Response.Redirect($"/Authorize{httpRequest.QueryString}");
                        context.HandleRequest();
                        return;
                }
            }
        }
    }
}
