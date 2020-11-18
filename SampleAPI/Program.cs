using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using SampleAPI.Extensions;

using Serilog;

namespace SampleAPI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog(LoggingExtensions.ConfigureSerilog)
                .ConfigureAppConfiguration(
                    (hostingContext, config) => { config.AddEnvironmentVariables(); })
                .ConfigureWebHostDefaults(
                    webHostBuilder =>
                        {
                            webHostBuilder
                                .ConfigureKestrel(
                                    options =>
                                        {
                                            options.Limits.MaxRequestBodySize = 200000000; // Max upload size is 200 Mb
                                            options.AddServerHeader = false;
                                        })
                                .UseContentRoot(Directory.GetCurrentDirectory())
                                .UseIISIntegration()
                                .UseStartup<Startup>();

                            webHostBuilder.ConfigureServices(services => { services.AddControllers(); });
                        });
    }
}