using static OpenIddict.Abstractions.OpenIddictConstants;
using System.Security.Claims;

namespace OpeniddictServer.Extensions
{
    public static class OpeniddictExtensions
    {
        public static IEnumerable<string> GetDestinations(Claim claim)
        {
            switch (claim.Type)
            {
                case ClaimTypes.Name:
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;

                    yield break;

                case ClaimTypes.Role:
                    yield return Destinations.AccessToken;
                    yield return Destinations.IdentityToken;

                    yield break;

                // Never include the security stamp in the access and identity tokens, as it's a secret value.
                case "AspNet.Identity.SecurityStamp": 
                    yield break;
            }
        }
    }
}
