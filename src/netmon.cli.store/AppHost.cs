using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using netmon.cli;
using netmon.cli.monitor;
using netmon.domain.Interfaces;
using netmon.domain.Models;

internal class AppHost : BaseAppHost
{
    private readonly IRestorageOrchestrator<PingResponseModel> _storageOrchestrator;

    public AppHost(IServiceCollection services, IHostEnvironment hostingEnvironment, string[] args): base(services, hostingEnvironment, args)
    {
        var configurationRoot = BootstrapConfiguration(hostingEnvironment.EnvironmentName, args).Build();
        ServiceProvider = BootstrapApplication(services, configurationRoot).BuildServiceProvider();
        _storageOrchestrator = ServiceProvider.GetRequiredService<IRestorageOrchestrator<PingResponseModel>>();

    }

    /// <summary>
    /// Sets up all the application modules in the Dependency injection container.
    /// </summary>
    /// <param name="configurationRoot">Uses the configuration to assist in setup.</param>
    /// <returns></returns>
    protected override IServiceCollection BootstrapApplication(IServiceCollection services, IConfigurationRoot configurationRoot)
    {
        Options = configurationRoot.GetSection("AppOptions").Get<AppOptions>();
        if (!Options.Monitor.StorageFolder.Exists) throw new ArgumentException("OutputPath");

        services = base.BootstrapApplication(services, configurationRoot);
        services
                .AddAppFileStorage(Options)
                .AddAppObjectStorage(Options)
            ;
        return services;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {

        Logger.LogTrace("Moving Files from .. {path} to storage service at {connection}",
                                                Options.Monitor.OutputPath,
                                                Options.Storage.ConnectionString
                                                 );

        while (!cancellationToken.IsCancellationRequested) // until cancelled
        {
            await _storageOrchestrator.MoveFilesToObjectStorage(cancellationToken);

            await Task.Delay(10000, cancellationToken); // wait 10 seconds and start again.
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        Logger.LogTrace("Stopping...");
        return Task.FromResult(0);
    }
}