using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Identity.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(
                    (hostingContext, config) =>
                        {
                            config.AddEnvironmentVariables();
                            if (hostingContext.HostingEnvironment.IsDevelopment())
                            {
                                config.AddUserSecrets<Program>();
                            }
                        })
                .ConfigureWebHostDefaults(
                    webBuilder =>
                        {
                            webBuilder.UseStartup<Startup>();
                            webBuilder.ConfigureKestrel(
                                options =>
                                    {
                                        options.AddServerHeader = false;
                                    });
                        });
    }
}