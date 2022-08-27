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
        private readonly IEnumerable<IMonitorSubOrchestrator> _monitors;
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
            _monitors = _serviceProvider.GetServices<IMonitorSubOrchestrator>(); // get em all
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

            var storageDirectory = new DirectoryInfo(_options.OutputPath);
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
                        return new PingResponseModelJsonRepository(storageDirectory, provider.GetRequiredService<JsonSerializerSettings>(), _options.FolderDelimiter);
                    })
                    .AddSingleton<IRepository>(provider =>
                    {
                        return new PingResponseModelTextSummaryRepository(storageDirectory, _options.FolderDelimiter);
                    })
                    .AddSingleton<IStorageOrchestrator<PingResponseModel>>(
                        (provider) =>
                        {
                            var respositories = provider.GetServices<IRepository>(); // get em all
                            var logger = provider.GetRequiredService<ILogger<PingResponseModelStorageOrchestrator>>();
                            return new PingResponseModelStorageOrchestrator(respositories, logger, provider.GetRequiredService<JsonSerializerSettings>());
                        })
                    .AddSingleton<IMonitorSubOrchestrator, MonitorTraceRouteThenPingSubOrchestrator>()
                    .AddSingleton<IMonitorSubOrchestrator, MonitorTraceRouteSubOrchestrator>()
                    .AddSingleton<IMonitorSubOrchestrator, MonitorPingSubOrchestrator>()
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

            foreach (var monitor in _monitors)
            {
                monitor.Reset += Monitor_Reset;
            }


            while (_continuing)
            {
                await _monitorOrchestrator.Execute(_options.Mode,
                                                    _options.IPAddresses,
                                                    _options.Until,
                                                    cancellationToken);
                _continuing = false;

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);
                }
            }


            foreach (var monitor in _monitors)
            {
                monitor.Reset -= Monitor_Reset;
            }

        }

        private void Monitor_Reset(object? sender, SubOrchestratorEventArgs e)
        {
            if (_resetTime.AddMinutes(2) > DateTimeOffset.UtcNow) return; // ignore repeats too soon
            lock (_resettingObject)
            {
                if (_continuing) return; // already done
                _continuing = true;
                _resetTime = DateTimeOffset.UtcNow;
                _cancellationTokenSource.Cancel();
            }
            Thread.Sleep(_cancellationThreadSleepTime);
        }

     

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Stopping...");
            return Task.FromResult(0);
        }
    }

}