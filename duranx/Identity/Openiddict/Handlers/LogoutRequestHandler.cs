using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using OpenIddict.Server;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace OpeniddictServer.Handlers
{
    public class LogoutRequestHandler : IOpenIddictServerHandler<HandleLogoutRequestContext>
    {
        public async ValueTask HandleAsync(HandleLogoutRequestContext context)
        {
            var httpRequest = context.Transaction.GetHttpRequest() ??
                throw new InvalidOperationException("HTTP request cannot be retrieved.");

            var httpContext = httpRequest.HttpContext ??
                throw new InvalidOperationException("HTTP context cannot be retrieved.");

            var request = httpContext.GetOpenIddictServerRequest() ??
                throw new InvalidOperationException("OpenID Connect request cannot be retrieved.");

            // Sign out the user
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //await httpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            context.HandleRequest();

            // Redirect to the post-logout redirect URI
            var postLogoutRedirectUri = context.Request.PostLogoutRedirectUri;
            if (!string.IsNullOrEmpty(postLogoutRedirectUri))
            {
                httpContext.Response.Redirect(postLogoutRedirectUri);
            }
            return;
        }
    }
}
