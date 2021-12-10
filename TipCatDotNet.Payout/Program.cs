using System;
using System.Threading.Tasks;
using HappyTravel.ConsulKeyValueClient.ConfigurationProvider.Extensions;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.StdOutLogger.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Payout.Infrastructure;
using TipCatDotNet.Payout.Infrastructure.Constants;

namespace TipCatDotNet.Payout
{
    internal static class Program
    {
        private static async Task Main()
        {
            await Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(builder =>
                {
                    builder
                        .UseKestrel()
                        .UseStartup<Startup>();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    var env = context.HostingEnvironment;

                    config.AddConsulKeyValueClient(
                        Environment.GetEnvironmentVariable(Common.ConsulEndpointEnvironmentVariableName) ??
                        throw new InvalidOperationException("Consul endpoint is not set"),
                        key: Common.ServiceName,
                        Environment.GetEnvironmentVariable(Common.ConsulTokenEnvironmentVariableName) ??
                        throw new InvalidOperationException("A Consul http token is not set"),
                        bucketName: env.EnvironmentName, delayOnFailureInSeconds: 60);

                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddEnvironmentVariables();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders()
                        .AddConfiguration(context.Configuration.GetSection("Logging"));

                    var env = context.HostingEnvironment;
                    if (env.IsLocal())
                        logging.AddConsole();
                    else
                    {
                        logging.AddStdOutLogger(setup =>
                        {
                            setup.IncludeScopes = true;
                            setup.RequestIdHeader = Constants.DefaultRequestIdHeader;
                            setup.UseUtcTimestamp = true;
                        });
                    }
                })
                .Build()
                .RunAsync();
        }
    }
}
