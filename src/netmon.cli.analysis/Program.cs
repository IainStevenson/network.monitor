using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace netmon.analysis
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) => {
                    services.AddHostedService<AppHost>((provider) => {
                        return new AppHost(services, context.HostingEnvironment, args);
                    });
                })
                .Build()
                .Run();
        }
    }
}