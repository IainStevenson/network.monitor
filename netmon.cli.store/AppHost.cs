using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MongoDB.Bson.Serialization;
using netmon.cli;
using netmon.core.Configuration;
using netmon.core.Interfaces;
using netmon.core.Interfaces.Repositories;
using netmon.core.Models;
using netmon.core.Orchestrators;
using netmon.core.Serialisation;
using netmon.core.Storage;
using Newtonsoft.Json;

internal class AppHost : IHostedService
{
    private readonly ServiceProvider _serviceProvider;
    private AppOptions _options;
    private readonly ILogger<AppHost> _logger;
    private readonly IRestorageOrchestrator<PingResponseModel> _storageOrchestrator;
    public AppHost(IServiceCollection services, IHostEnvironment hostingEnvironment, string[] args)
    {
        _options = new();
        var configurationRoot = BootstrapConfiguration(hostingEnvironment.EnvironmentName, args).Build();
        _serviceProvider = BootstrapApplication(services, configurationRoot);
        _logger = _serviceProvider.GetRequiredService<ILogger<AppHost>>();
        _storageOrchestrator = _serviceProvider.GetRequiredService<IRestorageOrchestrator<PingResponseModel>>();

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
            //cm.MapIdMember( c=>c.Id);
            cm.SetIgnoreExtraElements(true);
        });

        _options = configurationRoot.GetSection("AppOptions").Get<AppOptions>();



        if (!_options.StorageFolder.Exists) throw new ArgumentException("OutputPath");

        services
            .AddLogging(configure =>
                                {
                                    configure.AddConfiguration(configurationRoot);
                                    configure.AddSimpleConsole(options =>
                                    {
                                        options.ColorBehavior = LoggerColorBehavior.Enabled;
                                        options.SingleLine = true;
                                        options.IncludeScopes = false;
                                        options.UseUtcTimestamp = true;

                                    });
                                })
            .AddSingleton(x =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new IPAddressConverter());
                settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                settings.Converters.Add(new HostAdddresAndTypeConverter());
                settings.Formatting = Formatting.Indented;
                return settings;

            })
            .AddSingleton<IFileSystemRepository>(provider =>
            {
                return new PingResponseModelJsonRepository(_options.StorageFolder,
                        provider.GetRequiredService<JsonSerializerSettings>(),
                        _options.FolderDelimiter);
            })
            .AddSingleton<IStorageRepository<Guid, PingResponseModel>>(provider =>
            {
                var client = new MongoDB.Driver.MongoClient(_options.StorageService.ConnectionString);
                var database = client.GetDatabase("netmon");
                var collection = database.GetCollection<PingResponseModel>("ping");
                return new PingResponseModelObjectRepository(collection);
            })
            .AddSingleton<IRestorageOrchestrator<PingResponseModel>, PingResponseModelReStorageOrchestrator>()
            .AddSingleton(provider =>
                {
                    var instance = new Dictionary<MonitorModes, IMonitorSubOrchestrator>();
                    instance.Add(MonitorModes.PingOnly, new MonitorPingSubOrchestrator(
                            provider.GetRequiredService<IStorageOrchestrator<PingResponseModel>>(),
                            provider.GetRequiredService<ITraceRouteOrchestrator>(), provider.GetRequiredService<IPingOrchestrator>(),
                            provider.GetRequiredService<ILogger<MonitorBaseSubOrchestrator>>()
                            ));
                    instance.Add(MonitorModes.TraceRoute, new MonitorTraceRouteSubOrchestrator(
                            provider.GetRequiredService<IStorageOrchestrator<PingResponseModel>>(),
                            provider.GetRequiredService<ITraceRouteOrchestrator>(), provider.GetRequiredService<IPingOrchestrator>(),
                            provider.GetRequiredService<ILogger<MonitorBaseSubOrchestrator>>()));
                    instance.Add(MonitorModes.TraceRouteThenPing, new MonitorTraceRouteThenPingSubOrchestrator(
                        provider.GetRequiredService<IStorageOrchestrator<PingResponseModel>>(),
                            provider.GetRequiredService<ITraceRouteOrchestrator>(), provider.GetRequiredService<IPingOrchestrator>(),
                            provider.GetRequiredService<ILogger<MonitorBaseSubOrchestrator>>()
                        ));
                    return instance;
                }
            )
            ;
        return services.BuildServiceProvider();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        _logger.LogTrace("Moving Files from .. {path} to {connection}",
                                                _options.OutputPath,
                                                _options.StorageService.ConnectionString
                                                 );

        while (!cancellationToken.IsCancellationRequested) // until cancelled
        {
            await _storageOrchestrator.MoveFilesToObjectStorage(cancellationToken);

            await Task.Delay(10000, cancellationToken); // wait 10 seconds and start again.
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Stopping...");
        return Task.FromResult(0);
    }
}