using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging;
using OpenIddict.Client;
using OpenIddict.Validation;
using OpeniddictServer.Configuration;
using OpeniddictServer.Data;
using OpeniddictServer.Extensions;
using OpeniddictServer.Handlers;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using static OpenIddict.Abstractions.OpenIddictConstants;
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
    options.ExpireTimeSpan = TimeSpan.FromHours(1);
    options.SlidingExpiration = true;
})
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
{
    // Configuraci�n OpenID Connect para el cliente ShoppingWeb
    // La URL del servidor de autorizaci�n OpenID Connect (OpeniddictServer)
    options.Authority = openIddictConfig["Issuer"];

    // El identificador del cliente registrado en el servidor OpenID Connect
    options.ClientId = shoppingClient!.ClientId;

    // El secreto del cliente registrado en el servidor OpenID Connect
    options.ClientSecret = shoppingClient.ClientSecret;

    // El tipo de respuesta solicitado al servidor de autorizaci�n
    options.ResponseType = Parameters.Code; // C�digo de autorizaci�n

    // Indica que se deben guardar los tokens en la sesi�n de autenticaci�n
    options.SaveTokens = true;

    // Alcances solicitados
    options.Scope.AddRange(OpeniddictServer.Extensions.ConfigurationExtensions.GetDefaultAuthorizationCodeScopes());
    options.Scope.AddRange(shoppingClient.Scopes);

    // Indica si se deben obtener los claims del endpoint de informaci�n del usuario
    options.GetClaimsFromUserInfoEndpoint = true;

    // Configuraci�n de validaci�n de tokens
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = JwtClaimTypes.Name, // El claim que se utilizar� como nombre del usuario
        RoleClaimType = JwtClaimTypes.Role, // El claim que se utilizar� como rol del usuario
        TokenDecryptionKey = new X509SecurityKey(encryptionCert),
        IssuerSigningKey = new X509SecurityKey(signingCert),
    };

    // La ruta a la que el servidor de autorizaci�n redirigir� despu�s de la autenticaci�n
    options.CallbackPath = "/signin-oidc";

    // La URL a la que el usuario ser� redirigido despu�s de cerrar sesi�n
    options.SignedOutRedirectUri = shoppingClient.PostLogoutRedirectUri!;
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

        options.UseAspNetCore()
               .EnableLogoutEndpointPassthrough();

        options.AddEventHandler<HandleTokenRequestContext>(options =>
            options.UseScopedHandler<TokenRequestHandler>());

        options.AddEventHandler<HandleAuthorizationRequestContext>(options =>
            options.UseScopedHandler<AuthorizationRequestHandler>());

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
