using DnsClient.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using netmon.domain.Configuration;
using netmon.domain.Handlers;
using netmon.domain.Interfaces;
using System;

namespace netmon.cli.monitor
{
    public class AppHost : BaseAppHost
    {
        private ServiceProvider _serviceProvider;
        private ILogger<AppHost> _logger;
        private readonly Dictionary<MonitorModes, IMonitorModeOrchestrator> _monitors;

        
        public AppHost(IServiceCollection services, IHostEnvironment environment, string[] args) : base(services, environment, args)
        {
            var configurationRoot = BootstrapConfiguration(environment.EnvironmentName, args).Build();

            _serviceProvider = BootstrapApplication(services, configurationRoot).BuildServiceProvider();
            _logger = _serviceProvider.GetRequiredService<ILogger<AppHost>>();
            _monitors = _serviceProvider.GetRequiredService<Dictionary<MonitorModes, IMonitorModeOrchestrator>>();

        }
        public override Task StartAsync(CancellationToken cancellationToken)
        {

            // "Temporary code whilst getting fix for ping problems in linux with low TTL"
            var canUseRawSockets = RawSocketPermissions.CanUseRawSockets(System.Net.Sockets.AddressFamily.InterNetwork);

            _logger.LogTrace("Can use Sockets On this host... {canUse}", canUseRawSockets);

            _logger.LogTrace("Monitoring... {addresses} {until} {mode}",
                                                    Options.Monitor.Addresses,
                                                    $"{DateTimeOffset.UtcNow.Add(Options.Monitor.Until):o}",
                                                    Options.Monitor.Mode
                                                     );

            _monitors[Options.Monitor.Mode].Execute(Options.Monitor.IPAddresses, Options.Monitor.Until, cancellationToken).Wait();

            return Task.CompletedTask;

        }



        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogTrace("Stopping...");
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

            services
                    .AddAppMonitoring()
                    .AddAppFileStorage(Options)
                    ;
            return services;

        }
    }

}