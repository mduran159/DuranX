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
                case Claims.Name:
                case Claims.Email:
                case Claims.Role:
                case "oi_scp":
                case "oi_aud":
                    //yield return TokenTypeHints.AccessToken;
                    //yield return TokenTypeHints.IdToken;
                    yield return TokenTypeHints.AuthorizationCode;
                    yield return TokenTypeHints.IdToken;
                    yield return TokenTypeHints.AccessToken;
                    break;
                default:
                    yield return Destinations.AccessToken;
                    break;
            }
        }
    }
}
