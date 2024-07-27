using IdentityServer.Data;
using OpenIddict.Abstractions;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace IdentityServer.HostedServices
{
    public class CartService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public CartService(IServiceProvider serviceProvider)
            => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();
            var scopeManager = scope.ServiceProvider.GetRequiredService<IOpenIddictScopeManager>();

            if (await manager.FindByClientIdAsync("cart_client") is null)
            {
                await manager.CreateAsync(new OpenIddictApplicationDescriptor
                {
                    ClientId = "cart_client",
                    ClientSecret = "388D45FA-B36B-4988-BA59-B187D329C208",
                    DisplayName = "Cart Service",
                    Permissions =
                    {
                        Permissions.Endpoints.Token,
                        Permissions.GrantTypes.ClientCredentials,
                        "scp:read_cart",
                        "scp:write_cart"
                    }
                });
            }

            // Ensure the scope exists
            if (await scopeManager.FindByNameAsync("read_cart") is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "read_cart",
                    Description = "A scope for read data from Cart API."
                });
            }
            if (await scopeManager.FindByNameAsync("write_cart") is null)
            {
                await scopeManager.CreateAsync(new OpenIddictScopeDescriptor
                {
                    Name = "write_cart",
                    Description = "A scope for write data from Cart API."
                });
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
