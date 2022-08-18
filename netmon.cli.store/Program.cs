using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using netmon.cli;

public class Program
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