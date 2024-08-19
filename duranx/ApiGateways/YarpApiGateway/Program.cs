using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using YarpApiGateway.Middlewares;
using YarpApiGateway.RequestTransforms;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
IdentityModelEventSource.ShowPII = true;

// Add services to the container.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
        .AddTransforms(transformBuilder =>
        {
            transformBuilder.RequestTransforms.Add(new ForwardAuthorizationHeaderTransform());
        });

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var jwtBearerConfig = configuration.GetSection("JwtBearer");
    options.Authority = configuration["OpenIddict:Issuer"];
    //options.Audience = "api"; // O la audiencia que correspondía
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["OpenIddict:Issuer"],
        IssuerSigningKey = new X509SecurityKey(new X509Certificate2(
                    jwtBearerConfig.GetSection("Certificates:Signing:Path").Value!,
                    jwtBearerConfig.GetSection("Certificates:Signing:Password").Value!
                )),
        TokenDecryptionKey = new X509SecurityKey(new X509Certificate2(
                    jwtBearerConfig.GetSection("Certificates:Encryption:Path").Value!,
                    jwtBearerConfig.GetSection("Certificates:Encryption:Password").Value!
                ))
    };
});


builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
});

var app = builder.Build();

// Use the custom rate limiting middleware
app.UseMiddleware<ClientRateLimitingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Map reverse proxy
app.MapReverseProxy();

app.Run();
