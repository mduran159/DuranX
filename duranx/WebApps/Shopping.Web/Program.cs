using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Shopping.Web;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
IdentityModelEventSource.ShowPII = true;
IdentityModelEventSource.LogCompleteSecurityArtifact = true;

// Add services to the container.
builder.Services.AddRazorPages();

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AuthHandler>();

builder.Services.AddRefitClient<IInventoryService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
    })
    .AddHttpMessageHandler<AuthHandler>();

builder.Services.AddRefitClient<ICartService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
    })
    .AddHttpMessageHandler<AuthHandler>(); 

builder.Services.AddRefitClient<IOrderService>()
    .ConfigureHttpClient(c =>
    {
        c.BaseAddress = new Uri(builder.Configuration["ApiSettings:GatewayAddress"]!);
    })
    .AddHttpMessageHandler<AuthHandler>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
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

    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = ClaimTypes.Role,
        ClockSkew = TimeSpan.FromMinutes(0)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDataProtection()
    .ProtectKeysWithCertificate(new X509Certificate2(builder.Configuration["ServerEncryptionCert:Path"]!, 
                                                     builder.Configuration["ServerEncryptionCert:Password"]));

builder.Services.AddScoped<IGoogleDriveService>(service => new GoogleDriveService(builder.Configuration));

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

// Configura las rutas de los controladores
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}");

app.MapRazorPages();

// Redirect to the "Home" page as the default
app.MapGet("/", async context =>
{
    context.Response.Redirect("/Home");
});

app.Run();
