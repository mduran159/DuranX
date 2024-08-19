namespace OpeniddictServer.Configuration
{
    public class ClientsConfiguration
    {
        public required GrantType GrantType { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
        public required string DisplayName { get; set; }
        public string? ConsentType { get; set; }
        public required List<string> Scopes { get; set; }
        public string? PostLogoutRedirectUri { get; set; }
        public string? RedirectUri { get; set; }
    }

    public enum GrantType
    {
        AuthorizationCode,
        ClientCredentials
    }
}
