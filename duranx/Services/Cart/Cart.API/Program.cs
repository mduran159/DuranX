using BuildingBlocks.Messaging.RabbitMQ;
using Microsoft.IdentityModel.Logging;
using Discount.Grpc;
using System.Security.Cryptography.X509Certificates;
using BuildingBlocks.Cache.Redis;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;

//wait for rabbit mq server to start to avoid it throw error logs
await Task.Delay(10000);

var builder = WebApplication.CreateBuilder(args);

//Application Services
var assembly = typeof(Program).Assembly;
var configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
IdentityModelEventSource.ShowPII = true;

builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

//Data Services
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName);
}).UseLightweightSessions();

builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.Decorate<ICartRepository, CachedCartRepository>();

//Async communication services
builder.Services.AddRabbitMQ(builder.Configuration);

// Configurar Redis con TLS
builder.Services.AddRedisCache(builder.Configuration);

// Grpc Services
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    return handler;
});

//Cross-Cutting Services
builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);

//Identity
var openiddictConf = configuration.GetSection("OpenIddict");
var client = openiddictConf.GetSection("Client");
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(new Uri(openiddictConf["Issuer"]!));

        options.AddAudiences(client["Audience"]!);

        options.UseIntrospection()
            .SetClientId(client["ClientId"]!)
            .SetClientSecret(client["ClientSecret"]!);

        options.UseSystemNetHttp();
        options.UseAspNetCore();
    });

// Configurar la autenticación global usando OpenIddict
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtBearerConfig = configuration.GetSection("JwtBearer");
    options.Authority = openiddictConf["Issuer"];
    options.Audience = client["Audience"];
    options.RequireHttpsMetadata = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = openiddictConf["Issuer"],
        ValidateAudience = true,
        ValidAudience = client["Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
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
    options.AddPolicy("CartReadable", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var scopeClaim = context.User.FindFirst("scope")?.Value;
            if (string.IsNullOrEmpty(scopeClaim))
                throw new Exception("There are no scopes for the current user");
            return scopeClaim.Contains("read_cart");
        });
    });

    options.AddPolicy("CartWritable", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var scopeClaim = context.User.FindFirst("scope")?.Value;
            if (string.IsNullOrEmpty(scopeClaim))
                throw new Exception("There are no scopes for the current user");
            return scopeClaim.Contains("write_cart");
        });
    });
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

//Middleware
app.MapCarter();
app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

app.Run();
