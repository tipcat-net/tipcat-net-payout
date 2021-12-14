using Microsoft.Extensions.DependencyInjection;
using TipCatDotNet.Payout.Services.Payout;
using TipCatDotNet.Payout.Services.HttpClients;
using System;
using Microsoft.Extensions.Configuration;
using TipCatDotNet.Payout.Infrastructure.Constants;
using Polly;
using System.Net.Http;
using Polly.Extensions.Http;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace TipCatDotNet.Payout.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient(Common.PayoutHttpClientName, httpClient =>
        {
            httpClient.BaseAddress = new Uri(configuration["BaseApiUrl"]);
        })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = CertificateAlwaysTrueValidator
            })
            .AddPolicyHandler(GetRetryPolicy());

        bool CertificateAlwaysTrueValidator(HttpRequestMessage requestMessage, X509Certificate2? certificate,
            X509Chain? chain, SslPolicyErrors sslErrors)
            => true;


        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
                .WaitAndRetryAsync(6, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
    }


    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddHostedService<HostedPayoutService>();
        services.AddTransient<IPayoutHttpClient, PayoutHttpClient>();
        return services;
    }
}