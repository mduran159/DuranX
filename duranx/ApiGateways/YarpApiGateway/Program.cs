using YarpApiGateway.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Use the custom rate limiting middleware
app.UseMiddleware<ClientRateLimitingMiddleware>();

// Map reverse proxy
app.MapReverseProxy();

app.Run();
