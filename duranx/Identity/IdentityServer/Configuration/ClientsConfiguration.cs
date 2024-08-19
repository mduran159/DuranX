namespace IdentityServer.Configuration
{
    public class ClientsConfiguration
    {
        public required GrantType GrantType { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string DisplayName { get; set; }
        public required List<string> Scopes { get; set; }
        public List<string>? PostLogoutRedirectUris { get; set; }
        public  List<string>? RedirectUris { get; set; }
    }

    public enum GrantType
    {
        AuthorizationCode,
        ClientCredentials
    }
}
