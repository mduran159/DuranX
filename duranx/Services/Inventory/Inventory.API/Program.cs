using BuildingBlocks.Messaging.RabbitMQ;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;
var configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
IdentityModelEventSource.ShowPII = true;

builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblies(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddMarten(opts =>
{
    opts.Connection(configuration.GetConnectionString("Database")!);
}).UseLightweightSessions();

//If environment is development then insert initial data
if (builder.Environment.IsDevelopment())
    builder.Services.InitializeMartenWith<InventoryInitialData>();

builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.AddHealthChecks().AddNpgSql(configuration.GetConnectionString("Database")!);

//Async communication services
builder.Services.AddRabbitMQ(builder.Configuration);

//Identity
var openiddictConf = configuration.GetSection("OpenIddict");
var client = openiddictConf.GetSection("Client");
var jwtBearerConfig = configuration.GetSection("JwtBearer");
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
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
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
    options.AddPolicy("InventoryReadable", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var scopeClaim = context.User.FindFirst("scope")?.Value;
            if (string.IsNullOrEmpty(scopeClaim))
                throw new Exception("There are no scopes for the current user");
            return scopeClaim.Contains("read_inventory");
        });
    });

    options.AddPolicy("InventoryWritable", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireAssertion(context =>
        {
            var scopeClaim = context.User.FindFirst("scope")?.Value;
            if (string.IsNullOrEmpty(scopeClaim))
                throw new Exception("There are no scopes for the current user");
            return scopeClaim.Contains("write_inventory");
        });
    });
});

var app = builder.Build();

// Configure middlewares
app.UseExceptionHandler(options => { });

app.UseHealthChecks("/health",
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

//Identity
app.UseAuthentication();
app.UseAuthorization();

app.MapCarter();

app.Run();