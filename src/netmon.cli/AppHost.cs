using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MongoDB.Bson.Serialization;
using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Orchestrators;
using netmon.core.Serialisation;
using netmon.core.Storage;
using Newtonsoft.Json;

namespace netmon.cli
{
    public class AppHost : IHostedService
    {
        private readonly ServiceProvider _serviceProvider;
        private readonly ILogger<AppHost> _logger;
        private readonly IMonitorOrchestrator _monitorOrchestrator;
        private AppOptions _options;

        public AppHost(IServiceCollection services, IHostEnvironment environment, string[] args)
        {
            _options = new AppOptions();
            var config = BootstrapConfiguration(environment.EnvironmentName, args).Build();
            _serviceProvider = BootstrapApplication(services, config);
            _monitorOrchestrator = _serviceProvider.GetRequiredService<IMonitorOrchestrator>();
            _logger = _serviceProvider.GetRequiredService<ILogger<AppHost>>();
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
                    .AddSingleton<PingHandlerOptions>() // the defaults are good here
                    .AddSingleton<TraceRouteOrchestratorOptions>()// the defaults are good here
                    .AddSingleton<PingOrchestratorOptions>() // the defaults are good here
                    .AddSingleton<IPingRequestModelFactory, PingRequestModelFactory>()
                    .AddTransient<IPingHandler, PingHandler>()
                    .AddSingleton<ITraceRouteOrchestrator, TraceRouteOrchestrator>()
                    .AddSingleton<IPingOrchestrator, PingOrchestrator>()
                    .AddSingleton<JsonSerializerSettings>( x=>
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
                                                    _options.IPAddresses,
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