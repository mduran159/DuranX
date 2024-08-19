using IdentityServer.Configuration;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityServer.Extensions
{
    public static class ConfigurationExtensions
    {
        public static HashSet<string> AddPermissions(this HashSet<string> permissions, List<string> scopes, GrantType grantType)
        {
            // Agregar permisos específicos del tipo de grant
            switch (grantType)
            {
                case GrantType.AuthorizationCode:
                    permissions.Add(Permissions.Endpoints.Authorization);
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.AuthorizationCode);
                    permissions.Add(Permissions.GrantTypes.RefreshToken);
                    permissions.Add(Permissions.ResponseTypes.Code);
                    break;

                case GrantType.ClientCredentials:
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.ClientCredentials);
                    break;
            }

            // Agregar scopes como permisos
            foreach (var scope in scopes)
            {
                permissions.Add($"{Permissions.Prefixes.Scope}{scope}");
            }
            return permissions;
        }


        public static HashSet<Uri> AddRedirectUris(this HashSet<Uri> redirectUris, List<string> configRedirectUris)
        {
            foreach (var client in configRedirectUris)
            {
                redirectUris.Add(new Uri(client));
            }
            return redirectUris;
        }

        public static HashSet<Uri> AddPostLogoutRedirectUris(this HashSet<Uri> postLogoutRedirectUris, List<string> configPostLogoutRedirectUris)
        {
            foreach (var client in configPostLogoutRedirectUris)
            {
                postLogoutRedirectUris.Add(new Uri(client));
            }
            return postLogoutRedirectUris;
        }

        public static ClientsConfiguration AddDefaultScopes(this ClientsConfiguration client, GrantType grantType)
        {
            if(grantType == GrantType.AuthorizationCode) {
                client.Scopes.AddRange(GetDefaultAuthorizationCodeScopes());
            }
            return client;
        }

        public static List<ScopesConfiguration> AddDefaultScopes(this List<ScopesConfiguration> scopes)
        {
            foreach (var scope in GetDefaultAuthorizationCodeScopes())
            {
                scopes.Add(new ScopesConfiguration() { Name = scope, Description = $"Default scope for {scope}" });
            }
            return scopes;
        }

        private static List<string> GetDefaultAuthorizationCodeScopes()
        {
            return new List<string>() {
                Scopes.Email,
                Scopes.Profile,
                Scopes.OpenId
            };
        }
    }
}
