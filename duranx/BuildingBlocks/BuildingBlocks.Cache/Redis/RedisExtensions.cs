using BuildingBlocks.Common.Validations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using System.Security.Cryptography.X509Certificates;

namespace BuildingBlocks.Cache.Redis
{
    public static class RedisExtensions
    {
        public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConfig = configuration.GetSection("Redis");

            var redisOptions = new ConfigurationOptions
            {
                EndPoints = { string.Format("{0}:{1}", redisConfig["Hostname"]!, redisConfig["Port"]!) },
                Ssl = true,
                //Password = redisConfig["Password"],
                SslProtocols = System.Security.Authentication.SslProtocols.Tls13,
                AbortOnConnectFail = false,
                Protocol = RedisProtocol.Resp3
            };

            redisOptions.CertificateSelection += delegate
            {
                return new X509Certificate2(redisConfig["CertPath"]!, redisConfig["CertPassword"]!);

            };

            redisOptions.CertificateValidation += (sender, certificate, chain, sslPolicyErrors) =>
                CertificateValidations.ValidateServerCertificateBasic(certificate, sslPolicyErrors, redisConfig["CACertPath"]!);

            var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
            services.AddSingleton<IConnectionMultiplexer>(multiplexer);

            services.AddSingleton<IDistributedCache>(new RedisCache(new RedisCacheOptions
            {
                Configuration = configuration.GetConnectionString("Redis"),
                InstanceName = "Cart:",
            }));

            return services;
        }
    }
}
