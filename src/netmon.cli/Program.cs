using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace netmon.cli
{
    public class Program
    {
        static void Main(string[] args)
        {

            var argumentsHandler = new ArgumentsHandler(args);

            Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services.AddSingleton(argumentsHandler);
                    services.AddHostedService<AppHost>( (provider) => {
                        return new AppHost(services,context.HostingEnvironment, argumentsHandler);
                    });
                })
                .Build()
                .Run();
        }
    }
}