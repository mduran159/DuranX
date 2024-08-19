using IdentityServer.Configuration;
using IdentityServer.Data;
using IdentityServer.Extensions;
using OpenIddict.Abstractions;

namespace IdentityServer.HostedServices
{
    public class ConfigurationHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public ConfigurationHostedService(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            var clients = _configuration.GetSection("OpenIddict:Clients").Get<List<ClientsConfiguration>>();
            foreach (var client in clients!)
            {
                if (await manager.FindByClientIdAsync(client.ClientId) is null)
                {
                    client.AddDefaultScopes(client.GrantType);

                    var applicationDescription = new OpenIddictApplicationDescriptor
                    {
                        ClientId = client.ClientId,
                        ClientSecret = client.ClientSecret,
                        DisplayName = client.DisplayName,
                    };

                    applicationDescription.Permissions.AddPermissions(client.Scopes, client.GrantType);

                    if (client.RedirectUris is not null) 
                        applicationDescription.RedirectUris.AddRedirectUris(client.RedirectUris);

                    if(client.PostLogoutRedirectUris is not null) 
                        applicationDescription.PostLogoutRedirectUris.AddPostLogoutRedirectUris(client.PostLogoutRedirectUris);

                    await manager.CreateAsync(applicationDescription);
                }
            }

            var scopes = _configuration.GetSection("OpenIddict:Scopes")
                                       .Get<List<ScopesConfiguration>>()!
                                       .AddDefaultScopes();

            foreach (var scopeConf in scopes!)
            {
                if (await scopeManager.FindByNameAsync(scopeConf.Name) is null)
                {
                    await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                    {
                        Name = scopeConf.Name,
                        Description = scopeConf.Description
                    });
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
