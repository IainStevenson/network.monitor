using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using netmon.cli;
using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Orchestrators;
using netmon.core.Storage;

internal class AppHost : IHostedService
{
    private IServiceCollection _services;
    private IHostEnvironment _hostingEnvironment;
    private AppOptions _options;
    private readonly ServiceProvider _serviceProvider;
    private readonly ILogger<AppHost> _logger;

    public AppHost(IServiceCollection services, IHostEnvironment hostingEnvironment, AppOptions options)
    {
        _services = services;
        _hostingEnvironment = hostingEnvironment;
        _options = options;

        var config = BootstrapConfiguration(hostingEnvironment.EnvironmentName).Build();
        _serviceProvider = BootstrapApplication(services, config);
    }
    /// <summary>
    /// Provides the necessary configuration builder
    /// </summary>
    /// <param name="environmentName">Optionally includes configuration for the specified environment.</param>
    /// <returns></returns>
    private static IConfigurationBuilder BootstrapConfiguration(string environmentName)
    {
        return new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile($"appSettings.json", false, true) // must have
                        .AddJsonFile($"appSettings{environmentName}.json", true, true); // could have
    }
    /// <summary>
    /// Sets up all the application modules in the Dependency injection container.
    /// </summary>
    /// <param name="config">Uses the configuration to assist in setup.</param>
    /// <returns></returns>
    private ServiceProvider BootstrapApplication(IServiceCollection services, IConfigurationRoot config)
    {
        var storageDirectory = new DirectoryInfo(_options.OutputPath);
        if (!storageDirectory.Exists) throw new ArgumentException("OutputPath");
        services.AddLogging(configure =>
                            {
                                configure.AddConfiguration(config);

                                configure.AddSimpleConsole(options =>
                                {
                                    options.ColorBehavior = LoggerColorBehavior.Enabled;
                                    options.SingleLine = true;
                                    options.IncludeScopes = false;
                                    options.UseUtcTimestamp = true;

                                });
                            })
                   //.AddSingleton<PingHandlerOptions>() // the defaults are good here
                   //.AddSingleton<TraceRouteOrchestratorOptions>()// the defaults are good here
                   //.AddSingleton<PingOrchestratorOptions>() // the defaults are good here
                   //.AddSingleton<IPingRequestModelFactory, PingRequestModelFactory>()
                   //.AddTransient<IPingHandler, PingHandler>()
                   //.AddSingleton<ITraceRouteOrchestrator, TraceRouteOrchestrator>()
                   //.AddSingleton<IPingOrchestrator, PingOrchestrator>()
                   .AddSingleton<IStorage<PingResponseModel>>(provider =>
                   {
                       return new PingResponseModelJsonFileStorage(storageDirectory, _options.FolderDelimiter);
                   })
                   .AddSingleton<IPingResponseModelStorageOrchestrator>(
                       (provider) =>
                       {
                           var respositories = provider.GetServices<IStorage<PingResponseModel>>();
                           var logger = provider.GetRequiredService<ILogger<PingResponseModelStorageOrchestrator>>();
                           return new PingResponseModelStorageOrchestrator(respositories, logger);
                       })
                   .AddSingleton<IMonitorOrchestrator, MonitorOrchestrator>()
                   ;
        return services.BuildServiceProvider();
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {


        _logger.LogTrace("Monitoring... {addresses} {until} {mode}",
                                                _options.Addresses,
                                                _options.Until,
                                                _options.Mode
                                                 );


        // execute functionality here....


        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Stopping...");
        return Task.FromResult(0);
    }
}