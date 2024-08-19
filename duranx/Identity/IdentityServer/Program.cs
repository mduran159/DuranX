using IdentityServer.Data;
using IdentityServer.Extensions;
using IdentityServer.HostedServices;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
IdentityModelEventSource.ShowPII = true;
IdentityModelEventSource.LogCompleteSecurityArtifact = true;

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(connectionString);
    options.UseOpenIddict();
});

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();

var openIddictConfig = builder.Configuration.GetSection("OpenIddict");
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

        options.SetIssuer(new Uri(openIddictConfig["Issuer"]));

        options.SetAuthorizationEndpointUris(openIddictConfig["AuthorizationEndpointUri"])
               .SetLogoutEndpointUris(openIddictConfig["LogoutEndpointUri"])
               .SetTokenEndpointUris(openIddictConfig["TokenEndpointUri"]);

        options.RegisterScopes(openIddictConfig.GetSection("Scopes").Get<string[]>());

        var signingCertificate = openIddictConfig.GetSection("Certificates:Signing");
        var encryptionCertificate = openIddictConfig.GetSection("Certificates:Encryption");
        options.AddSigningCertificate(new X509Certificate2(signingCertificate["Path"], signingCertificate["Password"]))
               .AddEncryptionCertificate(new X509Certificate2(encryptionCertificate["Path"], encryptionCertificate["Password"]));

        options.UseAspNetCore()
               .EnableAuthorizationEndpointPassthrough()
               .EnableLogoutEndpointPassthrough()
               .EnableTokenEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();
    });

// Configuración de protección de datos
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(openIddictConfig["DataProtectionKeyDir"]))
    .ProtectKeysWithCertificate(new X509Certificate2(openIddictConfig["Certificates:Encryption:Path"], openIddictConfig["Certificates:Encryption:Password"]));

builder.Services.AddHostedService<ConfigurationHostedService>();

builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(options =>
{
    options.MapControllers();
    options.MapRazorPages();
});

app.UseCors();

if (app.Environment.IsDevelopment())
{
    await app.InitializeDatabaseAsync();
}

app.Run();