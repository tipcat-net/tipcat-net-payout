using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using TipCatDotNet.Payout.Infrastructure.Constants;

namespace TipCatDotNet.Payout.Services.HttpClients;

public class PayoutHttpClient : IPayoutHttpClient
{
    public PayoutHttpClient(IHttpClientFactory clientFactory, IConfiguration configuration, DiagnosticSource diagnosticSource)
    {
        _configuration = configuration;
        _diagnosticSource = diagnosticSource;
        _httpClient = clientFactory.CreateClient(Common.PayoutHttpClientName);
    }


    public async Task Payout()
    {
        using var _ = _diagnosticSource.StartActivity(new Activity(nameof(Payout)), null);

        using var httpResponseMessage = await _httpClient.PostAsync(_configuration["PayoutEndpointUrl"], null!);

        httpResponseMessage.EnsureSuccessStatusCode();
    }


    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly DiagnosticSource _diagnosticSource;
}