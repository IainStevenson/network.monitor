using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using netmon.cli;

public class Program
{
    static void Main(string[] args)
    {

        var options = new AppOptions();
        options.FromArguments(args);
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) => {
                services.AddSingleton(options);
                services.AddHostedService<AppHost>((provider) => {
                    return new AppHost(services, context.HostingEnvironment, options);
                });
            })
            .Build()
            .Run();
    }
}