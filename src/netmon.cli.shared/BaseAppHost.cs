using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using netmon.domain.Serialisation;
using Newtonsoft.Json;

namespace netmon.cli.monitor
{

    public abstract class BaseAppHost : IHostedService
    {
        protected AppOptions Options = new();
        protected ILogger<BaseAppHost> Logger;
        protected ServiceProvider ServiceProvider;

        public BaseAppHost(IServiceCollection services, IHostEnvironment environment, string[] args)
        {
            var config = BootstrapConfiguration(environment.EnvironmentName, args).Build();
            ServiceProvider = BootstrapApplication(services, config).BuildServiceProvider();
            Logger = ServiceProvider.GetRequiredService<ILogger<BaseAppHost>>();            
        }

        /// <summary>
        /// Implements the apppliction startup.
        /// </summary>
        /// <param name="cancellationToken">The asnychronous cancellation token.</param>
        /// <returns>An instance of <see cref="Task"/></returns>
        public abstract Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Implements stopping the application.
        /// </summary>
        /// <param name="cancellationToken">The asnychronous cancellation token.</param>
        /// <returns>An instance of <see cref="Task"/></returns>
        public abstract Task StopAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Provides the necessary configuration providers for json configuration with environment optional, and command line arguments.
        /// </summary>
        /// <param name="environmentName">Optionally includes configuration for the specified environment file.</param>
        /// <param name="args">Optionally includes configuration via command line arguments.</param>
        /// <returns>An isntance of a <see cref="ConfigurationBuilder"/> from which to populate classes via the options pattern.</returns>
        protected virtual IConfigurationBuilder BootstrapConfiguration(string environmentName, string[] args)
        {
            return new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile($"appSettings.json", false, true) // must have
                            .AddJsonFile($"appSettings{environmentName}.json", true, true)
                            .AddEnvironmentVariables()
                            .AddCommandLine(args)
                            ;
        }


        /// <summary>
        /// Sets up all the application <see cref="AppOptions"/> and prepares the cross-cutting modules in the dependency injection container.
        /// </summary>
        /// <param name="configurationRoot">Uses the configuration to assist in setup.</param>
        /// <returns></returns>
        protected virtual IServiceCollection BootstrapApplication(IServiceCollection services, IConfigurationRoot configurationRoot)
        {

            Options = configurationRoot.GetSection("AppOptions").Get<AppOptions>();

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
                                    }
                    )
                    .AddSingleton<CancellationTokenSource>()
                    .AddSingleton<JsonSerializerSettings>(x =>
                    {
                        var settings = new JsonSerializerSettings();
                        settings.Converters.Add(new IPAddressConverter());
                        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
                        settings.Converters.Add(new HostAdddresAndTypeConverter());
                        settings.Formatting = Formatting.Indented;
                        return settings;

                    })
                    ;

            return services;
        }
    }
}