using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using TechNova.RMS.Infrastructure;

namespace TechNova.RMS.Cli
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            var services = new ServiceCollection();
            // register app services from infrastructure composition helper
            DependencyInjections.RegisterServices(services);

            // register CLI app runner
            services.AddTransient<ConsoleApp>();

            var provider = services.BuildServiceProvider();

            // resolve and run the console application. Program.cs remains the composition root only.
            var app = provider.GetRequiredService<ConsoleApp>();
            await app.RunAsync();

            return 0;
        }
    }
}
