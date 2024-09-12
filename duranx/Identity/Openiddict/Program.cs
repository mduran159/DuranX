using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using OpenIddict.Client;
using OpenIddict.Validation;
using OpeniddictServer.Configuration;
using OpeniddictServer.Data;
using OpeniddictServer.Extensions;
using OpeniddictServer.Handlers;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Server.OpenIddictServerEvents;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddDefaultIdentity<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultUI();

var openIddictConfig = builder.Configuration.GetSection("OpenIddict");
var clients = openIddictConfig.GetSection("Clients").Get<List<ClientsConfiguration>>()!;
var shoppingClient = clients.FirstOrDefault(client => client.ClientId == "shopping_client")!;

var signingCertificate = openIddictConfig.GetSection("Certificates:Signing");
var encryptionCertificate = openIddictConfig.GetSection("Certificates:Encryption");
var signingCert = new X509Certificate2(signingCertificate["Path"]!, signingCertificate["Password"]);
var encryptionCert = new X509Certificate2(encryptionCertificate["Path"]!, encryptionCertificate["Password"]);
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Login"; // Ruta para redirigir cuando el usuario no est� autenticado
    options.LogoutPath = "/Logout"; // Ruta para manejar el cierre de sesi�n
    options.AccessDeniedPath = "/AccessDenied"; // Ruta para acceso denegado
    options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<ApplicationDbContext>();
    })
    .AddServer(options =>
    {
        options.AllowClientCredentialsFlow()
                .AllowAuthorizationCodeFlow()
                .AllowRefreshTokenFlow();

        options.SetIssuer(new Uri(openIddictConfig["Issuer"]!));

        options.SetAuthorizationEndpointUris(openIddictConfig["AuthorizationEndpointUri"]!)
                .SetLogoutEndpointUris(openIddictConfig["LogoutEndpointUri"]!)
                .SetTokenEndpointUris(openIddictConfig["TokenEndpointUri"]!);

        options.RegisterScopes(openIddictConfig.GetSection("Scopes").Get<string[]>()!);

        options.AddSigningCertificate(new X509Certificate2(signingCertificate["Path"], signingCertificate["Password"]))
                .AddEncryptionCertificate(new X509Certificate2(encryptionCertificate["Path"]!, encryptionCertificate["Password"]));

        options.UseAspNetCore();

        options.AddEventHandler<HandleTokenRequestContext>(options =>
            options.UseScopedHandler<TokenRequestHandler>());

        options.AddEventHandler<HandleAuthorizationRequestContext>(options =>
            options.UseScopedHandler<AuthorizationRequestHandler>());

        options.AddEventHandler<HandleLogoutRequestContext>(options =>
            options.UseScopedHandler<LogoutRequestHandler>());

        options.SetAccessTokenLifetime(TimeSpan.FromMinutes(5))
               .SetRefreshTokenLifetime(TimeSpan.FromDays(1))
               .SetIdentityTokenLifetime(TimeSpan.FromMinutes(5));
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    })
    .AddClient(options =>
    {
        options.AllowAuthorizationCodeFlow();

        options.AddSigningCertificate(new X509Certificate2(signingCertificate["Path"]!, signingCertificate["Password"]))
                .AddEncryptionCertificate(new X509Certificate2(encryptionCertificate["Path"]!, encryptionCertificate["Password"]));

        options.UseAspNetCore();
        options.UseSystemNetHttp();

        options.AddRegistration(new OpenIddictClientRegistration
        {
            Issuer = new Uri(openIddictConfig["Issuer"]!),
            ProviderName = "ShoppingWeb",
            ProviderDisplayName = "Shopping Web Server",

            ClientId = shoppingClient.ClientId,
            ClientSecret = shoppingClient.ClientSecret,

            RedirectUri = new Uri(shoppingClient.RedirectUri!),
            PostLogoutRedirectUri = new Uri(shoppingClient.PostLogoutRedirectUri!)
        });
    });

//builder.Services.AddDataProtection()
//    .PersistKeysToFileSystem(new DirectoryInfo(openIddictConfig["DataProtectionKeyDir"]))
//    .ProtectKeysWithCertificate(new X509Certificate2(openIddictConfig["Certificates:Encryption:Path"], openIddictConfig["Certificates:Encryption:Password"]));

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHostedService<SeedClientsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
} else
{
    await app.InitializeDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/Login");
});

app.MapGet("/GetDecryptedToken", async (string token, [FromServices] OpenIddictValidationService service) =>
{
    var principal = await service.ValidateAccessTokenAsync(token);
    if (principal == null)
    {
        return Results.BadRequest(new { Error = "Invalid token" });
    }

    var claims = principal.Claims.Select(claim => claim.Type + ":" + claim.Value);
    return Results.Ok(new { Claims = claims, Token = token });
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

// Seeding data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    await SeedData.Initialize(services, userManager);
}

app.Run();
