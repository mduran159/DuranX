using BuildingBlocks.Messaging.MassTransit.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Tls.Mtls.Explore.Models;
using System.Net.Security;
using System.Reflection;
using System.Security.Authentication;

namespace BuildingBlocks.Messaging.MassTransit
{
    public class RabbitMQWithSSL : IRabbitMQ
    {

        private readonly IOptions<RabbitMQOptions> _rabbitMQOptions;
        private readonly ILogger _logger;

        public RabbitMQWithSSL(IOptions<RabbitMQOptions> rabbitMQOptions, ILogger logger)
        {
            _rabbitMQOptions = rabbitMQOptions;
            _logger = logger;
        }

        public void Connect()
        {
            try
            {
                string rabbitmqHostName = _rabbitMQOptions.Value.HostName;
                string rabbitmqUsername = _rabbitMQOptions.Value.UserName;
                string rabbitmqPassword = _rabbitMQOptions.Value.Password;

                string rabbitmqServerName = _rabbitMQOptions.Value.ServerName;
                string certificateFilePath = _rabbitMQOptions.Value.CertPath;
                string certificatePassphrase = _rabbitMQOptions.Value.CertPassphrase;

                bool.TryParse(_rabbitMQOptions.Value.MTLSEnabled, out bool mTLSEnabled);
                var factory = new ConnectionFactory();

                factory.Uri = new Uri($"amqps://{rabbitmqUsername}:{rabbitmqPassword}@{rabbitmqHostName}");


                // Note: This should NEVER be "localhost"
                factory.Ssl.ServerName = rabbitmqServerName;

                // Path to my .p12 file.
                factory.Ssl.CertPath = certificateFilePath;
                // Passphrase for the certificate file - set through OpenSSL
                factory.Ssl.CertPassphrase = certificatePassphrase;

                factory.Ssl.Enabled = true;

                // Make sure TLS 1.2 is supported & enabled by your operating system
                factory.Ssl.Version = SslProtocols.Tls12;

                // This is the default RabbitMQ secure port
                factory.Port = AmqpTcpEndpoint.UseDefaultPort;
                factory.VirtualHost = "/";
                factory.Ssl.AcceptablePolicyErrors = SslPolicyErrors.RemoteCertificateChainErrors | SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable;

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        _logger.Log($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - Successfully connected and opened a channel");
                        channel.QueueDeclare("rabbitmq-dotnet-test", false, false, false, null);
                        _logger.Log($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - Successfully declared a queue");
                        channel.QueueDelete("rabbitmq-dotnet-test");
                        _logger.Log($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - Successfully deleted a queue");
                    }
                }
            }
            catch (System.Exception ex)
            {
                var error = ex.ToString();
                _logger.Log($"{System.Reflection.MethodBase.GetCurrentMethod().Name} - {error}");
            }
        }
    }

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
                        });
                    });
                });
            });

            return services;
        }
    }
}