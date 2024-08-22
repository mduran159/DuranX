using IdentityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Shopping.Web;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;
IdentityModelEventSource.LogCompleteSecurityArtifact = true;

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddRefitClient<IInventoryService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddRefitClient<ICartService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>(); 

builder.Services.AddRefitClient<IOrderService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
    })
    .AddHttpMessageHandler<AuthHeaderHandler>(); ;

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Login"; // Redirige a esta ruta para el inicio de sesión
})
.AddOpenIdConnect(options =>
{
    var authenticationConfig = builder.Configuration.GetSection("Authentication");
    options.Authority = authenticationConfig["Authority"];
    options.ClientId = authenticationConfig["ClientId"];
    options.ClientSecret = authenticationConfig["ClientSecret"];
    options.ResponseType = "code";
    options.SaveTokens = true;

    // Agrega los scopes desde appsettings.json
    foreach (var scope in authenticationConfig.GetSection("Scopes").Get<string[]>()!)
    {
        options.Scope.Add(scope);
    }

    options.GetClaimsFromUserInfoEndpoint = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = JwtClaimTypes.Name,
        RoleClaimType = JwtClaimTypes.Role,
    };
    // Usa los valores de appsettings.json para CallbackPath y PostLogoutRedirectUri
    options.CallbackPath = new PathString("/signin-oidc");
    options.SignedOutRedirectUri = authenticationConfig["PostLogoutRedirectUri"]!;
});

builder.Services.AddAuthorization();

builder.Services.AddDataProtection()
    .ProtectKeysWithCertificate(new X509Certificate2(builder.Configuration["ServerEncryptionCert:Path"]!, 
                                                     builder.Configuration["ServerEncryptionCert:Password"]));

builder.Services.AddScoped<IDropboxService>(service => new DropboxService(builder.Configuration["Dropbox:Token"]!));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

// Redirect to the "Home" page as the default
app.MapGet("/", async context =>
{
    context.Response.Redirect("/Home");
});

app.Run();
