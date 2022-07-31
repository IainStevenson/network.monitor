using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Orchestrators;
using netmon.core.Storage;
using System.Net;

namespace netmon.cli
{

    /// <summary>
    /// Inspired by : https://stackoverflow.com/questions/5891538/listen-for-key-press-in-net-console-app
    /// </summary>
    public class Program
    {
        private const string Message = "{Severity} Log Thread Id {Id}";
        private static CancellationTokenSource? _cancellationTokenSource;
        private static IMonitorOrchestrator? _monitorOrchestrator;
        private static ServiceProvider _serviceProvider;

        private static ILogger<Program>? _logger;


        /// <summary>
        /// Constructs the console application.
        /// </summary>
        static Program()
        {
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            var configuration = BootstrapConfiguration(environmentName);

            var config = configuration.Build();

            _serviceProvider = BootstrapApplication(config);

            _logger = _serviceProvider.GetRequiredService<ILogger<Program>>();

            _cancellationTokenSource = _serviceProvider.GetRequiredService<CancellationTokenSource>();

            Console.CancelKeyPress += (sender, e) =>
                                    {
                                        _cancellationTokenSource.Cancel();   // cancel token
                                        Console.WriteLine("Exiting...");
                                        Environment.Exit(0);
                                    };

        }

        /// <summary>
        /// Provides the necessary configuration builder
        /// </summary>
        /// <param name="environmentName">Optionally includes configuration fot he specified environment.</param>
        /// <returns></returns>
        private static IConfigurationBuilder BootstrapConfiguration(string environmentName)
        {
            return new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile($"appSettings.json", false, true) // must have
                            .AddJsonFile($"appSettings{environmentName}.json", true, true); // could have
        }

        /// <summary>
        /// Sets up all the application modules in teh Dependency ibjection container.
        /// </summary>
        /// <param name="config">Uses the configuration to assist in setup.</param>
        /// <returns></returns>
        private static ServiceProvider BootstrapApplication(IConfigurationRoot config)
        {
            return new ServiceCollection()
                           .AddLogging(configure =>
                           {
                               configure.AddConfiguration(config);

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
                           .AddSingleton(provider => {
                               return new PingResponseModelJsonStorage(new DirectoryInfo(Environment.CurrentDirectory));
                           })
                           .AddSingleton(provider => {
                               return new PingResponseModelTextSummaryStorage(new DirectoryInfo(Environment.CurrentDirectory));
                           })
                           .AddSingleton(
                                (provider) =>
                                {
                                    return new PingResponseModelStorageOrchestrator(
                                         provider.GetServices<IStorage<PingResponseModel>>(),
                                         provider.GetRequiredService<ILogger<PingResponseModelStorageOrchestrator>>()
                                        );
                                })
                           .AddSingleton<IMonitorOrchestrator, MonitorOrchestrator>()
                           .BuildServiceProvider();
        }

        static void Main(string[] args)
        {
            _monitorOrchestrator = _serviceProvider.GetRequiredService<IMonitorOrchestrator>();

            var argumentsHandler = new ArgumentsHandler(args);

            TimeSpan until = new TimeSpan(DateTime.UtcNow.AddYears(10).Ticks);

            Task busyTask = new Task(BusyIndicator);
            Task monitorKeys = new Task(ReadKeys);


            Task monitorTask = new Task(async () => await _monitorOrchestrator.Execute(argumentsHandler.Addresses,
                until, argumentsHandler.PingOnly,
                _cancellationTokenSource.Token));

            var tasks = new Task[] { busyTask, monitorKeys, monitorTask };


            foreach (var task in tasks)
            {
                task.Start();
            }

            Task.WaitAll(tasks);

            Console.ReadKey();
        }

        private static void BusyIndicator()
        {
            var busy = new ConsoleBusyIndicator();
            busy.UpdateProgress();
        }

        private static void ReadKeys()
        {
            ConsoleKeyInfo key = new();

            while (!Console.KeyAvailable && key.Key != ConsoleKey.Escape)
            {

                key = Console.ReadKey(true);

                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        Console.WriteLine("UpArrow was pressed");
                        break;
                    case ConsoleKey.DownArrow:
                        Console.WriteLine("DownArrow was pressed");
                        break;

                    case ConsoleKey.RightArrow:
                        Console.WriteLine("RightArrow was pressed");
                        break;

                    case ConsoleKey.LeftArrow:
                        Console.WriteLine("LeftArrow was pressed");
                        break;

                    case ConsoleKey.Escape:
                        _cancellationTokenSource?.Cancel();
                        break;

                    default:
                        //if (Console.CapsLock && Console.NumberLock)
                        //{
                        //    Console.WriteLine(key.KeyChar);
                        //}
                        break;
                }
            }
        }
    }

}