using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MongoDB.Bson.Serialization;
using netmon.domain.Configuration;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using netmon.domain.Serialisation;
using netmon.domain.Storage;
using Newtonsoft.Json;

namespace netmon.cli.monitor
{
    public class AppHost : IHostedService
    {
        private readonly object _resettingObject = new();
        private bool _continuing = true;
        private DateTimeOffset _resetTime;
        private const int _cancellationThreadSleepTime = 500;
        private readonly ServiceProvider _serviceProvider;
        private readonly ILogger<AppHost> _logger;
        private readonly IEnumerable<IMonitorModeOrchestrator> _monitors;
        private readonly IMonitorOrchestrator _monitorOrchestrator;
        private AppOptions _options;
        private readonly CancellationTokenSource _cancellationTokenSource;

        public AppHost(IServiceCollection services, IHostEnvironment environment, string[] args)
        {
            _options = new AppOptions();
            var config = BootstrapConfiguration(environment.EnvironmentName, args).Build();
            _serviceProvider = BootstrapApplication(services, config);
            _monitorOrchestrator = _serviceProvider.GetRequiredService<IMonitorOrchestrator>();
            _logger = _serviceProvider.GetRequiredService<ILogger<AppHost>>();
            _monitors = _serviceProvider.GetServices<IMonitorModeOrchestrator>(); // get em all
            _cancellationTokenSource = _serviceProvider.GetRequiredService<CancellationTokenSource>();
        }

        /// <summary>
        /// Provides the necessary configuration builder
        /// </summary>
        /// <param name="environmentName">Optionally includes configuration for the specified environment.</param>
        /// <returns></returns>
        private static IConfigurationBuilder BootstrapConfiguration(string environmentName, string[] args)
        {
            return new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile($"appSettings.json", false, true) // must have
                            .AddJsonFile($"appSettings{environmentName}.json", true, true)
                            .AddCommandLine(args)
                            ;
        }

        /// <summary>
        /// Sets up all the application modules in the Dependency injection container.
        /// </summary>
        /// <param name="configurationRoot">Uses the configuration to assist in setup.</param>
        /// <returns></returns>
        private ServiceProvider BootstrapApplication(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            BsonClassMap.RegisterClassMap<PingResponseModel>(cm =>
            {
                cm.AutoMap();
                //cm.MapIdMember(c => c.Id);
                cm.SetIgnoreExtraElements(true);
            });

            _options = configurationRoot.GetSection("AppOptions").Get<AppOptions>();

            var storageDirectory = new DirectoryInfo(_options.Capture.OutputPath);
            if (!storageDirectory.Exists) throw new ArgumentException("OutputPath");

            services.AddLogging(configure =>
                    {
                        configure.AddConfiguration(configurationRoot);

                        configure.AddSimpleConsole(options =>
                        {
                            options.ColorBehavior = LoggerColorBehavior.Enabled;
                            options.SingleLine = true;
                            options.IncludeScopes = false;
                            options.UseUtcTimestamp = true;

                        });
                    }
                    )
                    .AddSingleton<CancellationTokenSource>()
                    .AddSingleton<PingHandlerOptions>() // the defaults are good here
                    .AddSingleton<TraceRouteOrchestratorOptions>()// the defaults are good here
                    .AddSingleton<PingOrchestratorOptions>() // the defaults are good here
                    .AddSingleton<IPingRequestModelFactory, PingRequestModelFactory>()
                    .AddTransient<IPingHandler, PingHandler>()
                    .AddSingleton<ITraceRouteOrchestrator, TraceRouteOrchestrator>()
                    .AddSingleton<IPingOrchestrator, PingOrchestrator>()
                    .AddSingleton<JsonSerializerSettings>(x =>
                    {
                        var settings = new JsonSerializerSettings();
                        settings.Converters.Add(new IPAddressConverter());
                        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                        settings.Converters.Add(new HostAdddresAndTypeConverter());
                        settings.Formatting = Formatting.Indented;
                        return settings;

                    })
                    .AddSingleton<IRepository>(provider =>
                    {
                        return new PingResponseModelJsonRepository(
                            storageDirectory,
                            provider.GetRequiredService<JsonSerializerSettings>(),
                            _options.Capture.FolderDelimiter,
                            provider.GetRequiredService<ILogger<PingResponseModelJsonRepository>>());
                    })
                    .AddSingleton<IRepository>(provider =>
                    {
                        return new PingResponseModelTextSummaryRepository(storageDirectory, _options.Capture.FolderDelimiter);
                    })
                    .AddSingleton<IMonitorModeOrchestrator, TraceRouteContinuouslyOrchestrator>()
                    .AddSingleton<IMonitorModeOrchestrator, PingContinuouslyOrchestrator>()
                    .AddSingleton<IMonitorOrchestrator, MonitorOrchestrator>()
                    .AddSingleton<IMonitorModeOrchestrator, TraceRouteThenPingContinuouslyOrchestrator>()
                    .AddSingleton<IEnumerable<IRepository>>((provider) => { return provider.GetServices<IRepository>(); })
                    .AddSingleton(provider =>
                    {
                        var instance = new Dictionary<MonitorModes, IMonitorModeOrchestrator>
                            {
                                {
                                    MonitorModes.PingContinuously,
                                    new PingContinuouslyOrchestrator(
                                    provider.GetRequiredService<IPingOrchestrator>(),
                                    provider.GetRequiredService<PingOrchestratorOptions>(),
                                    provider.GetRequiredService<ILogger<PingContinuouslyOrchestrator>>()
                                    )
                                },
                                {
                                    MonitorModes.TraceRouteContinuously,
                                    new TraceRouteContinuouslyOrchestrator(
                                        provider.GetRequiredService<ITraceRouteOrchestrator>(),
                                        provider.GetRequiredService<ILogger<TraceRouteContinuouslyOrchestrator>>()
                                        )
                                },
                                {
                                    MonitorModes.TraceRouteThenPingContinuously,
                                    new TraceRouteThenPingContinuouslyOrchestrator(
                                    provider.GetRequiredService<ITraceRouteOrchestrator>(),
                                    provider.GetRequiredService<IPingOrchestrator>(),
                                    provider.GetRequiredService<ILogger<TraceRouteThenPingContinuouslyOrchestrator>>()
                                )
                                }
                            };
                        return instance;
                        }
                        )
                    ;

            return services.BuildServiceProvider();

        }


        public async Task StartAsync(CancellationToken cancellationToken)
        {

            // "Temporary code whilst getting fix for ping problems in linux with low TTL"
            var canUseRawSockets = RawSocketPermissions.CanUseRawSockets(System.Net.Sockets.AddressFamily.InterNetwork);

            _logger.LogTrace("Can use Sockets On this host... {canUse}", canUseRawSockets);

            _logger.LogTrace("Monitoring... {addresses} {until} {mode}",
                                                    _options.Capture.Addresses,
                                                    _options.Capture.Until,
                                                    _options.Capture.Mode
                                                     );

            while (_continuing)
            {
                await _monitorOrchestrator.Execute(_options.Capture.Mode,
                                                    _options.Capture.IPAddresses,
                                                    _options.Capture.Until,
                                                    cancellationToken);
                _continuing = false;

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }

        }



        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Stopping...");
            return Task.FromResult(0);
        }
    }

}