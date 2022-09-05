using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;

namespace netmon.cli.monitor
{
    public class AppHost : BaseAppHost
    {
        private readonly Dictionary<MonitorModes, IMonitorModeOrchestrator> _monitors;

        
        public AppHost(IServiceCollection services, IHostEnvironment environment, string[] args) : base(services, environment, args)
        {
            var configurationRoot = BootstrapConfiguration(environment.EnvironmentName, args).Build();

            ServiceProvider = BootstrapApplication(services, configurationRoot).BuildServiceProvider();           

            _monitors = ServiceProvider.GetRequiredService<Dictionary<MonitorModes, IMonitorModeOrchestrator>>();

        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {

            // "Temporary code whilst getting fix for ping problems in linux with low TTL"
            var canUseRawSockets = RawSocketPermissions.CanUseRawSockets(System.Net.Sockets.AddressFamily.InterNetwork);

            Logger.LogTrace("Can use Sockets On this host... {canUse}", canUseRawSockets);

            Logger.LogTrace("Monitoring... {addresses} {until} {mode}",
                                                    Options.Monitor.Addresses,
                                                    $"{DateTimeOffset.UtcNow.Add(Options.Monitor.Until):o}",
                                                    Options.Monitor.Mode
                                                     );

            _monitors[Options.Monitor.Mode].Execute(Options.Monitor.IPAddresses, Options.Monitor.Until, cancellationToken).Wait();

            return Task.CompletedTask;

        }



        public override Task StopAsync(CancellationToken cancellationToken)
        {
            Logger.LogTrace("Stopping...");
            return Task.CompletedTask;
        }



        /// <summary>
        /// Sets up all of the applications sepcific modules in the Dependency injection container by calling the base for cross-cutng, 
        /// and then adding in optional modules from the domain.
        /// </summary>
        /// <param name="configurationRoot">Uses the configuration to assist in setup.</param>
        /// <returns></returns>
        protected override IServiceCollection BootstrapApplication(IServiceCollection services, IConfigurationRoot configurationRoot)
        {
            
            Options = configurationRoot.GetSection("AppOptions").Get<AppOptions>();
            Options.Monitor.EnsureStorageDirectoryExists(Options.Monitor.OutputPath);
            if (!Options.Monitor.StorageFolder.Exists) throw new ArgumentException("OutputPath");

            services = base.BootstrapApplication(services,configurationRoot);
            services
                    .AddAppMonitoring()
                    .AddAppFileStorage(Options)
                    ;
            return services;

        }
    }

}