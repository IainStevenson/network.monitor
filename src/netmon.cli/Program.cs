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
        private static readonly CancellationTokenSource? _cancellationTokenSource;
        private static readonly IMonitorOrchestrator? _monitorOrchestrator;
        private static readonly ILogger<Program>? _logger;
        static void Main(string[] args)
        {
            var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

            var configuration = new ConfigurationBuilder()
                             .SetBasePath(Directory.GetCurrentDirectory())
                             .AddJsonFile($"appSettings.json")
                             .AddJsonFile($"appSettings{environmentName}.json",true,true);

            var config = configuration.Build();

            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddFilter("Microsoft", LogLevel.Warning)
                    .AddFilter("System", LogLevel.Warning)
                    .AddFilter("netmon", LogLevel.Trace)
                    //.AddFilter("netmon.cli.Program", LogLevel.Trace)
                    .AddSimpleConsole(config =>
                    {
                        config.SingleLine = true;
                    });
            });
            ILogger logger = loggerFactory.CreateLogger<Program>();
            Task.Run(() => LogMessages(logger)).Wait();
            Console.ReadKey();
        }

        private static void LogMessages(ILogger logger)
        {
            List<Task> tasks = new List<Task>() {
            Task.Run(() => LogLowMessages(logger)),
            Task.Run(() => LogHighMessages(logger))
        };

            Task.WaitAll(tasks.ToArray());

        }
        private static void LogLowMessages(ILogger logger)
        {
            logger.LogTrace(Message, "Trace", Environment.CurrentManagedThreadId);
            logger.LogDebug(Message, "Debug", Environment.CurrentManagedThreadId);

        }

        private static void LogHighMessages(ILogger logger)
        {
            logger.LogInformation(Message, "Info", Environment.CurrentManagedThreadId);
            logger.LogWarning(Message, "Warning", Environment.CurrentManagedThreadId);
            logger.LogError(Message, "Error", Environment.CurrentManagedThreadId);
            logger.LogCritical(Message, "Critical", Environment.CurrentManagedThreadId);
        }



        //    var serviceProvider = new ServiceCollection()
        //               .AddLogging(configure =>
        //                   {
        //                       configure.AddConfiguration(config);

        //                       configure.AddSimpleConsole(options =>
        //                       {
        //                           options.ColorBehavior = LoggerColorBehavior.Enabled;
        //                           options.SingleLine = true;
        //                           options.IncludeScopes = false;
        //                           options.UseUtcTimestamp = true;

        //                       });


        //                       //configure.AddTraceSource("netmon.cli");

        //                   }
        //               )
        //               .AddSingleton<PingHandlerOptions>() // the defaults are good here
        //               .AddSingleton<TraceRouteOrchestratorOptions>()// the defaults are good here
        //               .AddSingleton<PingOrchestratorOptions>() // the defaults are good here
        //               .AddSingleton<IPingRequestModelFactory, PingRequestModelFactory>()
        //               .AddTransient<IPingHandler, PingHandler>()
        //               .AddSingleton<ITraceRouteOrchestrator, TraceRouteOrchestrator>()
        //               .AddSingleton<IPingOrchestrator, PingOrchestrator>()
        //               .AddSingleton<IStorage<PingResponseModel>, PingResponseModelInMemoryStorage>()
        //               .AddSingleton<IMonitorOrchestrator, MonitorOrchestrator>()
        //               .AddSingleton<CancellationTokenSource>()
        //               .BuildServiceProvider();


        //    var arguments = args.ToList();

        //    _logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<Program>();
        //    _logger.LogInformation("Starting application");
        //    try
        //    {



        //        _cancellationTokenSource = serviceProvider.GetRequiredService<CancellationTokenSource>();
        //        _monitorOrchestrator = serviceProvider.GetRequiredService<IMonitorOrchestrator>();

        //        Console.CancelKeyPress += (sender, e) =>
        //        {
        //            // cancel token
        //            _cancellationTokenSource.Cancel();
        //            Console.WriteLine("Exiting...");
        //            Environment.Exit(0);
        //        };

        //        _logger.LogInformation("Press ESC to Exit");


        //        var taskKeys = new Task(ReadKeys);
        //        taskKeys.Start();

        //        //var taskProcessFiles = new Task(ProcessFiles);
        //        //taskProcessFiles.Start();

        //        var taskBusy = new Task(BusyIndicator);
        //        var taskMonitor = new Task(async () => await _monitorOrchestrator.Execute(new List<IPAddress>(),
        //                                        new TimeSpan(99, 23, 59, 59, 999),
        //                                        false,
        //                                        _cancellationTokenSource.Token));

        //        taskBusy.Start(); // on its own thread 
        //        taskMonitor.Start(); // on its own thread - and other generated threads


        //        var tasks = new[] { taskMonitor, taskKeys };

        //        Task.WaitAll(tasks);
        //        _logger.LogInformation("Tasks {status} .", (_cancellationTokenSource.IsCancellationRequested ? "cancelled" : "completed"));
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError("Application Exception: {message}", ex.Message);
        //    }
        //    _logger.LogInformation("Program complete.");
        //}



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

    internal class ConsoleBusyIndicator
    {
        int _currentBusySymbol;

        public char[] BusySymbols { get; set; }

        public ConsoleBusyIndicator()
        {
            BusySymbols = new[] { '|', '/', '-', '\\' };
        }
        public void UpdateProgress()
        {
            while (true)
            {
                Thread.Sleep(100);
                var originalX = Console.CursorLeft;
                var originalY = Console.CursorTop;

                Console.Write(BusySymbols[_currentBusySymbol]);

                _currentBusySymbol++;

                if (_currentBusySymbol == BusySymbols.Length)
                {
                    _currentBusySymbol = 0;
                }

                Console.SetCursorPosition(originalX, originalY);
            }
        }
    }
}