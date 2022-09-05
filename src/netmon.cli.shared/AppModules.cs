using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson.Serialization;
using netmon.domain.Configuration;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using netmon.domain.Interfaces.Repositories;
using netmon.domain.Models;
using netmon.domain.Orchestrators;
using netmon.domain.Storage;

namespace netmon.cli.monitor
{
    public static class AppModules
    {

        /// <summary>
        /// Adds the necessary basic monitoring services to perform PING operations. 
        /// Plaase note, at least one <see cref="IStorageRepository{TKey, TItem}"/> must also be cofigured for use.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to populate.</param>
        /// <returns>The same <see cref="IServiceCollection"/> intance.</returns>
        public static IServiceCollection AddAppMonitoring(this IServiceCollection services)
        {
            services.AddSingleton<PingHandlerOptions>() // the defaults are good here
                    .AddSingleton<PingOrchestratorOptions>() // the defaults are good here
                    .AddSingleton<TraceRouteOrchestratorOptions>()// the defaults are good here
                    .AddSingleton<IPingRequestModelFactory, PingRequestModelFactory>()
                    .AddTransient<IPinOrchestrator, PingHandler>()
                    .AddSingleton<ITraceRouteOrchestrator, TraceRouteOrchestrator>()
                    .AddSingleton<IPingOrchestrator, PingOrchestrator>()
                    .AddSingleton<PingContinuouslyOrchestrator>()
                    .AddSingleton<TraceRouteContinuouslyOrchestrator>()
                    .AddSingleton<TraceRouteThenPingContinuouslyOrchestrator>()
                    .AddSingleton(provider =>
                    {
                        var instance = new Dictionary<MonitorModes, IMonitorModeOrchestrator>
                            {
                                {
                                    MonitorModes.PingContinuously,
                                    provider.GetRequiredService<PingContinuouslyOrchestrator>()
                                },
                                {
                                    MonitorModes.TraceRouteContinuously,
                                    provider.GetRequiredService<TraceRouteContinuouslyOrchestrator>()
                                },
                                {
                                    MonitorModes.TraceRouteThenPingContinuously,
                                    provider.GetRequiredService<TraceRouteThenPingContinuouslyOrchestrator>()
                                }
                            };
                        return instance;
                    })
                    .AddSingleton<IMonitorOrchestrator, MonitorOrchestrator>()
                    ;
            return services;
        }

        /// <summary>
        /// Adds object storage repositories and Orchestrators.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to populate.</param>
        /// <param name="options">The already configured <see cref="AppOptions"/> instance to use.</param>
        /// <returns>The same <see cref="IServiceCollection"/> intance.</returns>
        public static IServiceCollection AddAppObjectStorage(this IServiceCollection services, AppOptions options)
        {
            services.AddSingleton<IStorageRepository<Guid, PingResponseModel>>(provider =>
            {
                var client = new MongoDB.Driver.MongoClient(options.Storage.ConnectionString);
                var database = client.GetDatabase(options.Storage.DatabaseName);
                var collection = database.GetCollection<PingResponseModel>("ping");
                return new PingResponseModelObjectRepository(collection, provider.GetRequiredService<ILogger<PingResponseModelObjectRepository>>());
            })
           .AddSingleton<IRestorageOrchestrator<PingResponseModel>, PingResponseModelReStorageOrchestrator>()
            // Add new storage services here
            ;
            return services;
        }

        /// <summary>
        /// Provides BSON mapping setup for all apps requiring MongoDB storage access
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to populate.</param>
        /// <returns>The same <see cref="IServiceCollection"/> intance.</returns>
        public static IServiceCollection AddAppStorageBsonMapping(this IServiceCollection services)
        {
            BsonClassMap.RegisterClassMap<PingResponseModel>(cm =>
            {
                cm.AutoMap();
                //cm.MapIdMember(c => c.Id);
                cm.SetIgnoreExtraElements(true);
            });
            return services;
        }

        /// <summary>
        /// Add the repositories for file storage, both JSON and text.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> instance to populate.</param>
        /// <param name="options">The already configured <see cref="AppOptions"/> instance to use.</param>
        /// <returns>The same <see cref="IServiceCollection"/> intance.</returns>
        public static IServiceCollection AddAppFileStorage(this IServiceCollection services, AppOptions options)
        {
            services.AddSingleton<IRepository>(provider =>
                                                {
                                                    return provider.GetRequiredService<PingResponseModelJsonRepository>();
                                                }
                    )
                    .AddSingleton<IRepository>(provider =>
                    {
                        return new PingResponseModelTextSummaryRepository(options.Monitor.StorageFolder, options.Monitor.FolderDelimiter);
                    })

                ;
            return services;
        }

    }

}