using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TipCatDotNet.Payout.Services.HttpClients;

namespace TipCatDotNet.Payout.Services.Payout
{
    public class HostedPayoutService : BackgroundService
    {
        public HostedPayoutService(IPayoutHttpClient payoutHttpClient, IServiceProvider services, IHostApplicationLifetime applicationLifetime,
            ILogger<HostedPayoutService> logger, DiagnosticSource diagnosticSource)
        {
            _applicationLifetime = applicationLifetime;
            _services = services;
            _payoutHttpClient = payoutHttpClient;
            _logger = logger;
            _diagnosticSource = diagnosticSource;
        }


        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            using (var scope = _services.CreateScope())
            {
                using var payoutActivity = _diagnosticSource.StartActivity(new Activity(nameof(HostedPayoutService)), null);

                try
                {
                    await _payoutHttpClient.Payout();
                }
                catch (Exception e)
                {
                    _logger.LogCritical(e, $"Error occurred: '{e.Message}'");
                }

                _applicationLifetime.StopApplication();
            }
        }


        private IServiceProvider _services { get; }
        private readonly IPayoutHttpClient _payoutHttpClient;
        private readonly ILogger<HostedPayoutService> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly DiagnosticSource _diagnosticSource;
    }
}
