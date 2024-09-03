using OpeniddictServer.Configuration;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpeniddictServer.Extensions
{
    public static class ConfigurationExtensions
    {
        public static HashSet<string> AddPermissions(this HashSet<string> permissions, ClientsConfiguration client)
        {
            // Agregar permisos específicos del tipo de grant
            switch (client.GrantType)
            {
                case GrantType.AuthorizationCode:
                    permissions.Add(Permissions.Endpoints.Authorization);
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.AuthorizationCode);
                    permissions.Add(Permissions.GrantTypes.RefreshToken);
                    permissions.Add(Permissions.ResponseTypes.Code);
                    permissions.Add(Permissions.Endpoints.Logout);
                    break;

                case GrantType.ClientCredentials:
                    permissions.Add(Permissions.Endpoints.Token);
                    permissions.Add(Permissions.GrantTypes.ClientCredentials);
                    break;
            }

            // Agregar scopes como permisos
            foreach (var scope in client.Scopes)
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
                scopes.Add(new ScopesConfiguration() { Name = scope, Description = $"Default scope for {scope}", Audience = new List<string>() });
            }
            return scopes;
        }

        public static List<string> GetDefaultAuthorizationCodeScopes()
        {
            return new List<string>() {
                Scopes.Email,
                Scopes.Profile,
                Scopes.OpenId
            };
        }
    }
}
