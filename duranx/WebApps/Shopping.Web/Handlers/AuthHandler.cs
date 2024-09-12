using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Headers;

namespace Shopping.Web
{
    public class AuthHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;

        public AuthHandler(IHttpContextAccessor httpContextAccessor, HttpClient httpClient)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = httpClient;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            var accessToken = await httpContext.GetTokenAsync("access_token");
            var refreshToken = await httpContext.GetTokenAsync("refresh_token");

            if (!string.IsNullOrEmpty(accessToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                // Intenta refrescar el token
                var tokenResponse = await RefreshAccessTokenAsync(refreshToken);

                if (tokenResponse.IsError)
                {
                    // Redirige al usuario a la página de inicio de sesión
                    httpContext.Response.Redirect("/Account/Login");
                    return response;
                }

                // Guarda el nuevo access token y refresh token
                accessToken = tokenResponse.AccessToken;
                refreshToken = tokenResponse.RefreshToken;

                // Actualiza el contexto de autenticación
                var authResult = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                var newAuthProperties = authResult.Properties!;
                newAuthProperties.Items[".Token.access_token"] = tokenResponse.AccessToken;
                newAuthProperties.Items[".Token.refresh_token"] = tokenResponse.RefreshToken;
                newAuthProperties.Items[".Token.expires_at"] = DateTimeOffset.Parse(newAuthProperties.Items[".Token.expires_at"]!).AddSeconds(tokenResponse.ExpiresIn).ToString("o");

                // Actualiza el cookie de autenticación con los nuevos tokens                          
                await httpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    authResult.Principal!,
                    newAuthProperties
                );

                // Reintenta la solicitud con el nuevo access token
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                response = await base.SendAsync(request, cancellationToken);
            }
            return response;
        }

        private async Task<TokenResponse> RefreshAccessTokenAsync(string refreshToken)
        {
            var discoveryDocument = await _httpClient.GetDiscoveryDocumentAsync("https://identityserver:6066/.well-known/openid-configuration");

            if (discoveryDocument.IsError)
            {
                // Manejar error
                throw new Exception("Error retrieving discovery document");
            }

            var refreshTokenResponse = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = discoveryDocument.TokenEndpoint,
                ClientId = "shopping_client",
                ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C210",
                RefreshToken = refreshToken
            });

            return refreshTokenResponse;
        }
    }
}
