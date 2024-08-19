using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Order.Application.Data;
using System.Security.Cryptography.X509Certificates;

namespace Order.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices
        (this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        // Add services to the container.
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        var openiddictConf = configuration.GetSection("OpenIddict");
        var client = openiddictConf.GetSection("Client");
        var jwtBearerConfig = configuration.GetSection("JwtBearer");
        services.AddOpenIddict()
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
        services.AddAuthentication(options =>
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

        services.AddAuthorization(options =>
        {
            options.AddPolicy("OrderReadable", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var scopeClaim = context.User.FindFirst("scope")?.Value;
                    if (string.IsNullOrEmpty(scopeClaim))
                        throw new Exception("There are no scopes for the current user");
                    return scopeClaim.Contains("read_order");
                });
            });

            options.AddPolicy("OrderWritable", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireAssertion(context =>
                {
                    var scopeClaim = context.User.FindFirst("scope")?.Value;
                    if (string.IsNullOrEmpty(scopeClaim))
                        throw new Exception("There are no scopes for the current user");
                    return scopeClaim.Contains("write_order");
                });
            });
        });

        return services;
    }
}
