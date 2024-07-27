using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace BuildingBlocks.Messaging.MassTransit;
public static class Extensions
{
    public static IServiceCollection AddMessageBroker
        (this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            if (assembly != null)
                config.AddConsumers(assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                configurator.Host(new Uri(configuration["MessageBroker:Host"]!), host =>
                {
                    host.Username(configuration["MessageBroker:UserName"]);
                    host.Password(configuration["MessageBroker:Password"]);
                    
                    host.UseSsl(options =>
                    {
                        options.CertificatePath = configuration["MessageBroker:Certificate:Path"]!;
                        options.CertificatePassphrase = configuration["MessageBroker:Certificate:Password"]!;
                        options.ServerName = new Uri(configuration["MessageBroker:Host"]!).Host;
                        options.Protocol = SslProtocols.Tls13;
                        options.UseCertificateAsAuthenticationIdentity = true;
                    });
                });
            });
        });

        return services;
    }
}
