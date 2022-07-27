using Microsoft.Extensions.DependencyInjection;
using netmon.core.Configuration;
using netmon.core.Handlers;
using netmon.core.Interfaces;
using netmon.core.Models;
using netmon.core.Orchestrators;
using netmon.core.Storage;
using System.Net;

public static class Program
{

    private static CancellationTokenSource cancellationTokenSource;
    private static IMonitorOrchestrator monitorOrchestrator;
    static void Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
                   .AddLogging()
                   .AddSingleton<PingHandlerOptions>() // the defaults are good here
                   .AddSingleton<TraceRouteOrchestratorOptions>()// the defaults are good here
                   .AddSingleton<PingOrchestratorOptions>() // the defaults are good here
                   .AddSingleton<IPingRequestModelFactory, PingRequestModelFactory>()
                   .AddTransient<IPingHandler, PingHandler>()
                   .AddSingleton<ITraceRouteOrchestrator, TraceRouteOrchestrator>()
                   .AddSingleton<IPingOrchestrator, PingOrchestrator>()
                   .AddSingleton<IStorage<PingResponseModel>, PingResponseModelInMemoryStorage>()
                   .AddSingleton<IMonitorOrchestrator, MonitorOrchestrator>()
                   .AddSingleton<CancellationTokenSource>()
                   .BuildServiceProvider();

        cancellationTokenSource = serviceProvider.GetRequiredService<CancellationTokenSource>();
        monitorOrchestrator = serviceProvider.GetRequiredService<IMonitorOrchestrator>();

        Console.CancelKeyPress += (sender, e) =>
        {
            // cancel token
            cancellationTokenSource.Cancel();   
            Console.WriteLine("Exiting...");
            Environment.Exit(0);
        };

        Console.WriteLine("Press ESC to Exit");


        var taskKeys = new Task(ReadKeys);
        taskKeys.Start();

        //var taskProcessFiles = new Task(ProcessFiles);
        //taskProcessFiles.Start();

        var taskMonitor = new Task( async () => await monitorOrchestrator.Execute(new List<IPAddress>(),
                                        new TimeSpan(99,23,59,59,999),
                                        false,
                                        cancellationTokenSource.Token));

        taskMonitor.Start();

        var tasks = new[] { taskKeys };
        Task.WaitAll(tasks);
    }   

    private static void ProcessFiles()
    {
        var files = Enumerable.Range(1, 100).Select(n => "File" + n + ".txt");

        var taskBusy = new Task(BusyIndicator);
        taskBusy.Start();

        foreach (var file in files)
        {
            Thread.Sleep(1000);
            Console.WriteLine("Procesing file {0}", file);
        }
    }

    private static void BusyIndicator()
    {
        var busy = new ConsoleBusyIndicator();
        busy.UpdateProgress();
    }

    private static void ReadKeys()
    {
        ConsoleKeyInfo key = new ConsoleKeyInfo();

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
                    break;

                default:
                    if (Console.CapsLock && Console.NumberLock)
                    {
                        Console.WriteLine(key.KeyChar);
                    }
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

//// See https://aka.ms/new-console-template for more information


//var monitorOrchestrator = serviceProvider.GetRequiredService<IMonitorOrchestrator>();

//if (args.Length == 0)
//{
//   

//    }



//}


//return 1;