using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MongoDB.Bson.Serialization;
using netmon.cli;
using netmon.domain.Configuration;
using netmon.domain.Interfaces;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using netmon.domain.Serialisation;
using netmon.domain.Storage;
using Newtonsoft.Json;

internal class AppHost : IHostedService
{
    private readonly ServiceProvider _serviceProvider;
    private AppOptions _options;
    private readonly ILogger<AppHost> _logger;
    private readonly IAnalysisOrchestrator<PingResponseModel> _orchestrator;
    public AppHost(IServiceCollection services, IHostEnvironment hostingEnvironment, string[] args)
    {
        _options = new();
        var configurationRoot = BootstrapConfiguration(hostingEnvironment.EnvironmentName, args).Build();
        _serviceProvider = BootstrapApplication(services, configurationRoot);
        _logger = _serviceProvider.GetRequiredService<ILogger<AppHost>>();
        _orchestrator = _serviceProvider.GetRequiredService<IAnalysisOrchestrator<PingResponseModel>>();

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


        if (!_options.Monitor.StorageFolder.Exists) throw new ArgumentException("OutputPath");

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
                return new PingResponseModelJsonRepository(_options.Monitor.StorageFolder,
                        provider.GetRequiredService<JsonSerializerSettings>(),
                        _options.Monitor.FolderDelimiter, 
                        provider.GetRequiredService<ILogger<PingResponseModelJsonRepository>>());
            })
            .AddSingleton<IStorageRepository<Guid, PingResponseModel>>(provider =>
            {
                var client = new MongoDB.Driver.MongoClient(_options.Storage.ConnectionString);
                var database = client.GetDatabase("netmon");
                var collection = database.GetCollection<PingResponseModel>("ping");
                return new PingResponseModelObjectRepository(collection, 
                    provider.GetRequiredService<ILogger<PingResponseModelObjectRepository>>());
            })
            .AddSingleton<IRestorageOrchestrator<PingResponseModel>, PingResponseModelReStorageOrchestrator>()
           
            ;
        return services.BuildServiceProvider();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {

        _logger.LogTrace("Analysing Raw data ... From {from} to {to} at {connection}",
                                                _options.Analysis.FromTime,
                                                _options.Analysis.ToTime,
                                                _options.Storage.ConnectionString
                                                 );

        await _orchestrator.Execute(_options.Analysis.FromTime,
                                    _options.Analysis.ToTime,
                                    cancellationToken);

        _logger.LogTrace("Analysis complete.");

    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogTrace("Stopping...");
        return Task.FromResult(0);
    }
}