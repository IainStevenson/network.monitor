using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Orchestrators;
using netmon.core.Storage;

namespace netmon.cli
{
    public class AppHost : IHostedService
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ILogger<AppHost> _logger;
        private readonly IMonitorOrchestrator _monitorOrchestrator;
        private readonly AppOptions _options;
        public AppHost(IServiceCollection services, IHostEnvironment environment, AppOptions options)
        {
            var config = BootstrapConfiguration(environment.EnvironmentName).Build();
            _serviceProvider = BootstrapApplication(services, config);
            _options = options;
            _logger = _serviceProvider.GetRequiredService<ILogger<AppHost>>();
            _monitorOrchestrator = _serviceProvider.GetRequiredService<IMonitorOrchestrator>();
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
        private static DirectoryInfo EnsureStorageDirectoryExits(string folderDelimiter, string outputPath)
        {
            
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                var commonDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                outputPath = $"{commonDataFolder}{folderDelimiter}netmon";

            }
            var storageDirectory = new DirectoryInfo(outputPath);
            if (!storageDirectory.Exists)
            {
                storageDirectory.Create();
            }
            return storageDirectory;
        }
        /// <summary>
        /// Sets up all the application modules in the Dependency injection container.
        /// </summary>
        /// <param name="config">Uses the configuration to assist in setup.</param>
        /// <returns></returns>
        private ServiceProvider BootstrapApplication(IServiceCollection services, IConfigurationRoot config)
        {



            var folderDelimiter = Environment.OSVersion.Platform == PlatformID.Unix ? "/" : "\\";
            var storageDirectory = EnsureStorageDirectoryExits(folderDelimiter, _options.OutputPath);

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
                    }
                    )
                    .AddSingleton<PingHandlerOptions>() // the defaults are good here
                    .AddSingleton<TraceRouteOrchestratorOptions>()// the defaults are good here
                    .AddSingleton<PingOrchestratorOptions>() // the defaults are good here
                    .AddSingleton<IPingRequestModelFactory, PingRequestModelFactory>()
                    .AddTransient<IPingHandler, PingHandler>()
                    .AddSingleton<ITraceRouteOrchestrator, TraceRouteOrchestrator>()
                    .AddSingleton<IPingOrchestrator, PingOrchestrator>()
                    .AddSingleton<IStorage<PingResponseModel>>(provider =>
                    {
                        return new PingResponseModelJsonStorage(storageDirectory, folderDelimiter);
                    })
                    .AddSingleton<IStorage<PingResponseModel>>(provider =>
                    {
                        return new PingResponseModelTextSummaryStorage(storageDirectory, folderDelimiter);
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

            //"Temporary code whilst getting fix for ping problems in linux with low TTL"
            var canUseRawSockets = RawSocketPermissions.CanUseRawSockets(System.Net.Sockets.AddressFamily.InterNetwork);
            _logger.LogTrace("Can use Sockets On this host... {canUse}", canUseRawSockets);

            _logger.LogTrace("Monitoring... {addresses} {until} {mode}",
                                                    _options.Addresses,
                                                    _options.Until,
                                                    _options.Mode
                                                     );


            await _monitorOrchestrator.Execute(_options.Mode,
                                                    _options.Addresses,
                                                    _options.Until,
                                                    cancellationToken);


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

}