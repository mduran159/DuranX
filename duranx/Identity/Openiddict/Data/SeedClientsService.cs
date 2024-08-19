using OpeniddictServer.Configuration;
using OpeniddictServer.Extensions;
using OpenIddict.Abstractions;

namespace OpeniddictServer.Data
{
    public class SeedClientsService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        public SeedClientsService(IServiceProvider serviceProvider, IConfiguration configuration)
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
                        DisplayName = client.DisplayName
                    };

                    if(client.ConsentType is not null) 
                        applicationDescription.ConsentType = client.ConsentType;

                    applicationDescription.Permissions.AddPermissions(client);

                    if (client.RedirectUri is not null)
                        applicationDescription.RedirectUris.Add(new Uri(client.RedirectUri));

                    if (client.PostLogoutRedirectUri is not null)
                        applicationDescription.PostLogoutRedirectUris.Add(new Uri(client.PostLogoutRedirectUri));
                    
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

                    var scopeDescription = new OpenIddictScopeDescriptor
                    {
                        Name = scopeConf.Name,
                        Description = scopeConf.Description
                    };

                    foreach (var resource in scopeConf.Audience)
                    {
                        scopeDescription.Resources.Add(resource);
                    }

                    await scopeManager.CreateAsync(scopeDescription);

                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
