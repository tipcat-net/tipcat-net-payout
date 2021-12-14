using System;
using System.Collections.Generic;
using HappyTravel.StdOutLogger.Extensions;
using HappyTravel.VaultClient;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TipCatDotNet.Payout.Infrastructure.Constants;
using TipCatDotNet.Payout.Infrastructure.Extensions;

namespace TipCatDotNet.Payout;

public class Startup
{
    public Startup(IConfiguration configuration, IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }


    public void ConfigureServices(IServiceCollection services)
    {
        services.AddHttpClients(_configuration);
        services.AddServices();
        services.AddHealthChecks();
        services.AddMemoryCache();
    }


    public static void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseHttpContextLogging(
            options => options.IgnoredPaths = new HashSet<string> { "/health" }
        );

        app.UseHealthChecks("/health");
    }


    private readonly IConfiguration _configuration;
    private IHostEnvironment _environment { get; }
}
