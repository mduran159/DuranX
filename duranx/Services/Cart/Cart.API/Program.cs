using BuildingBlocks.Messaging.MassTransit;
using Cart.API;
using Microsoft.IdentityModel.Logging;

//wait for rabbit mq server to start to avoid it throw error logs
await Task.Delay(8000);

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








//builder.Services.AddTransient<RabbitMQStartup>();
//builder.Services.AddTransient<IRabbitMQ, RabbitMQWithSSL>();

//builder.Services.AddOptions<RabbitMQOptions>()
//              .Configure<IConfiguration>((options, configuration) =>
//              {
//                  configuration.GetSection(RabbitMQOptions.SectionName).Bind(options);
//              });

//builder.Services.AddOptions<LoggerOptions>()
//             .Configure<IConfiguration>((options, configuration) =>
//             {
//                 configuration.GetSection(LoggerOptions.SectionName).Bind(options);
//             });
//builder.Services.AddSingleton(typeof(BuildingBlocks.Messaging.MassTransit.Interfaces.ILogger), typeof(ConsoleLogger));




//Async communication services
builder.Services.AddMessageBroker(builder.Configuration);








//var redisCertPath = builder.Configuration["Redis:CertPath"];
//var redisCertPassword = builder.Configuration["Redis:CertPassword"];
//var redisCert = new X509Certificate2(redisCertPath, redisCertPassword);

//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = builder.Configuration.GetConnectionString("Redis");
//    //options.ConfigurationOptions = new ConfigurationOptions
//    //{
//    //    EndPoints = { "localhost:6380" },
//    //    Ssl = true,
//    //    SslProtocols 
//    //    CertificateValidation = (sender, certificate, chain, sslPolicyErrors) => true, // O tu lógica de validación de certificados
//    //    ClientCertificates = new X509Certificate2Collection(redisCert)
//    //};
//});

// Grpc Services
//builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
//{
//    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
//})
//.ConfigurePrimaryHttpMessageHandler(() =>
//{
//    var handler = new HttpClientHandler
//    {
//        ServerCertificateCustomValidationCallback =
//        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
//    };

//    return handler;
//});

//Cross-Cutting Services
builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);
    //.AddRedis(builder.Configuration.GetConnectionString("Redis")!);

//Identity
//builder.Services.AddOpenIddict()
//    .AddValidation(options =>
//    {
//        options.SetIssuer(new Uri(configuration.GetSection("OpenIddict:Issuer").Value!));
//        options.UseSystemNetHttp();
//        options.UseAspNetCore();
//    });

// Configurar la autenticación global usando OpenIddict
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
//    options.AddPolicy("CartReadable", policy =>
//    {
//        policy.RequireClaim("scope", "read_cart");
//    });

//    options.AddPolicy("CartWritable", policy =>
//    {
//        policy.RequireClaim("scope", "write_cart");
//    });
//});

var app = builder.Build();

//Middleware
app.MapCarter();
app.UseExceptionHandler(options => { });

//var rabbitMQSartup = app.Services.GetRequiredService<RabbitMQStartup>();
//rabbitMQSartup.Run();
//app.UseHealthChecks("/health",
//    new HealthCheckOptions
//    {
//        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
//    });

app.Run();
