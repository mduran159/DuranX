using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.RabbitMQ
{
    public static class RabbitMQExtensions
    {
        public static IServiceCollection AddRabbitMQ
            (this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
        {
            services.AddMassTransit(config =>
            {
                config.SetKebabCaseEndpointNameFormatter();

                if (assembly != null)
                    config.AddConsumers(assembly);

                config.UsingRabbitMq((context, configurator) =>
                {
                    configurator.Host(new Uri(configuration["RabbitMQ:Host"]!), host =>
                    {
                        host.Username(configuration["RabbitMQ:UserName"]);
                        host.Password(configuration["RabbitMQ:Password"]);

                        host.UseSsl(options =>
                        {
                            options.CertificatePath = configuration["RabbitMQ:Certificates:CertPath"]!;
                            options.CertificatePassphrase = configuration["RabbitMQ:Certificates:CertPassword"]!;
                            options.ServerName = new Uri(configuration["RabbitMQ:Host"]!).Host;
                        });

                        configurator.ConfigureEndpoints(context);
                    });
                });
            });

            return services;
        }
    }
}
