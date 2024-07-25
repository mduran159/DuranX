using IdentityServer.Data;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityServer.HostedServices
{
    public class InventoryService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public InventoryService(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByClientIdAsync("inventoryClient") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "inventoryClient",
                    ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C207",
                    DisplayName = "Inventory Service",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                        "scp:read_inventory",
                        "scp:write_inventory"
                    }
                });
            }

            // Ensure the scope exists
            if (await scopeManager.FindByNameAsync("read_inventory") is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "read_inventory",
                    Description = "A scope for read data from Inventory API."
                });
            }
            if (await scopeManager.FindByNameAsync("write_inventory") is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "write_inventory",
                    Description = "A scope for write data from Inventory API."
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
