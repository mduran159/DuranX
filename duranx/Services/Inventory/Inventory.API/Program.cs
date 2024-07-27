using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Logging;

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

////Identity
//builder.Services.AddOpenIddict()
//    .AddValidation(options =>
//    {
//        options.SetIssuer(new Uri(configuration.GetSection("OpenIddict:Issuer").Value!));
//        options.UseSystemNetHttp();
//        options.UseAspNetCore();
//    });

//// Configurar la autenticación global usando OpenIddict
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    var jwtBearerConfig = configuration.GetSection("JwtBearer");
//    options.Authority = jwtBearerConfig["Authority"];
//    options.Audience = jwtBearerConfig["Audience"];
//    options.RequireHttpsMetadata = true;
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidAudience = jwtBearerConfig["Audience"],
//        ValidIssuer = jwtBearerConfig["Authority"],
//        IssuerSigningKey = new X509SecurityKey(new X509Certificate2(
//            jwtBearerConfig.GetSection("Certificates:Signing:Path").Value!,
//            jwtBearerConfig.GetSection("Certificates:Signing:Password").Value!
//        )),
//        TokenDecryptionKey = new X509SecurityKey(new X509Certificate2(
//            jwtBearerConfig.GetSection("Certificates:Encryption:Path").Value!,
//            jwtBearerConfig.GetSection("Certificates:Encryption:Password").Value!
//        ))
//    };
//});

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("InventoryReadable", policy =>
//    {
//        policy.RequireClaim("scope", "read_inventory");
//    });

//    options.AddPolicy("InventoryWritable", policy =>
//    {
//        policy.RequireClaim("scope", "write_inventory");
//    });
//});

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