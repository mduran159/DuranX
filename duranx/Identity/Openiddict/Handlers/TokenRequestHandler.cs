using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using OpenIddict.Server.AspNetCore;
using OpeniddictServer.Extensions;
using System.Collections.Immutable;
using System.Security.Claims;
using static OpenIddict.Abstractions.OpenIddictConstants;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace OpeniddictServer.Handlers
{
    public class TokenRequestHandler : IOpenIddictServerHandler<HandleTokenRequestContext>
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly IOpenIddictApplicationManager _applicationManager;
        private readonly IOpenIddictAuthorizationManager _authorizationManager;
        private readonly IOpenIddictTokenManager _tokenManager;
        private readonly IOpenIddictScopeManager _scopeManager;

        public TokenRequestHandler(
            UserManager<IdentityUser> userManager, 
            SignInManager<IdentityUser> signInManager,
            IOpenIddictApplicationManager applicationManager,
            IOpenIddictAuthorizationManager authorizationManager,
            IOpenIddictTokenManager tokenManager,
            IOpenIddictScopeManager scopeManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _applicationManager = applicationManager;
            _authorizationManager = authorizationManager;
            _tokenManager = tokenManager;
            _scopeManager = scopeManager;
        }
        async ValueTask IOpenIddictServerHandler<HandleTokenRequestContext>.HandleAsync(HandleTokenRequestContext context)
        {
            var application = _applicationManager.FindByClientIdAsync(context.Request.ClientId!).Result;
            
            if (application is null)
            {
                context.Reject(
                    error: Errors.InvalidClient,
                    description: "The client credentials are invalid.");
            }

            if (context.Request.IsClientCredentialsGrantType())
            {
                var identity = new ClaimsIdentity(authenticationType: TokenTypes.Bearer,
                                                  nameType: Claims.Name,
                                                  roleType: Claims.Role);
                identity.SetClaim(Claims.Subject, context.ClientId);
                identity.SetClaim(Claims.Name, await _applicationManager.GetDisplayNameAsync(application));
                identity.SetClaim(Claims.Audience, context.ClientId!.Replace("client", "audience"));
                identity.SetResources(await _scopeManager.ListResourcesAsync(identity.GetScopes()).ToListAsync());
                identity.SetDestinations(OpeniddictExtensions.GetDestinations);
                identity.SetScopes(context.Request.GetScopes());

                context.Principal = new ClaimsPrincipal(identity);
                return;
            }
            else if (context.Request.IsAuthorizationCodeGrantType() ||
                        context.Request.IsRefreshTokenGrantType())
            {
                var httpRequest = context.Transaction.GetHttpRequest() ??
                    throw new InvalidOperationException("HTTP request cannot be retrieved.");

                var httpContext = httpRequest.HttpContext ??
                    throw new InvalidOperationException("HTTP context cannot be retrieved.");
                // Retrieve the claims principal stored in the authorization code/device code/refresh token.
                var principal =
                    (await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme))
                    .Principal!;

                // Retrieve the user profile corresponding to the authorization code/refresh token.
                // Note: if you want to automatically invalidate the authorization code/refresh token
                // when the user password/roles change, use the following line instead:
                // var user = _signInManager.ValidateSecurityStampAsync(info.Principal);
                var user = await _userManager.GetUserAsync(principal);
                if (user == null)
                {
                    await httpContext.ForbidAsync(
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The token is no longer valid."
                        }));
                    context.HandleRequest();
                    return;
                }

                // Ensure the user is still allowed to sign in.
                if (!await _signInManager.CanSignInAsync(user!))
                {
                    await httpContext.ForbidAsync(
                        OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
                        properties: new AuthenticationProperties(new Dictionary<string, string?>
                        {
                            [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidGrant,
                            [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                                "The user is no longer allowed to sign in."
                        }));
                    context.HandleRequest();
                    return;
                }

                var permission = await _applicationManager.GetPermissionsAsync(application!);
                var scopes = permission.Where(item => item.StartsWith("scp:"))
                                        .Select(item => item.Substring(4)) // Remove "scp:" prefix
                                        .ToImmutableArray();
                var resources = await _scopeManager.ListResourcesAsync(scopes).ToListAsync();

                principal.SetClaim(Claims.Name, user.UserName);
                //principal.SetClaim(Claims.Role, string.Join(" ", await _userManager.GetRolesAsync(user)));
                //principal.SetClaim(Claims.Scope, string.Join(" ", scopes));
                principal.SetScopes(scopes);
                principal.SetResources(resources);

                foreach (var claim in principal.Claims)
                {
                    claim.SetDestinations(OpeniddictExtensions.GetDestinations(claim));
                }

                // Returning a SignInResult will ask OpenIddict to issue the appropriate access/identity tokens.
                await httpContext.SignInAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme, principal);
                context.HandleRequest();
                return;
            }
            else
            {
                context.Reject(
                    error: Errors.UnsupportedGrantType,
                    description: "The specified grant type is not supported.");
                return;
            }
        }
    }
}
