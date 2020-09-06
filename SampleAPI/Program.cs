using System.IO;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

//using SampleAPI.Security;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.SystemConsole.Themes;

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
            .UseSerilog(ConfigureSerilog)
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
                        webBuilder.ConfigureKestrel(
                            options =>
                                {
                                    options.Limits.MaxRequestBodySize = 5000000; // Max upload size is 5 Mb
                                    options.AddServerHeader = false;
                                    //options.ConfigureEndpoints();
                                });
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseIISIntegration();
                        webBuilder.UseSerilog();
                    });

        private static void ConfigureSerilog(HostBuilderContext hc, LoggerConfiguration config)
        {
            var logConfig = config
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", "KidstarterAPI");

            var consoleTemplate = hc.Configuration["Logging:ConsoleOutputTemplate"];
            if (!string.IsNullOrEmpty(consoleTemplate))
            {
                logConfig.WriteTo.Console(
                    outputTemplate: consoleTemplate,
                    theme: AnsiConsoleTheme.Literate);
            }

            var graylogEnabled = bool.Parse(hc.Configuration["Graylog:Enabled"]);
            if (graylogEnabled)
            {
                var graylogHost = hc.Configuration["Graylog:Host"];
                var graylogPort = int.Parse(hc.Configuration["Graylog:Port"]);

                config.WriteTo.Graylog(
                    new GraylogSinkOptions
                    {
                        HostnameOrAddress = graylogHost,
                        Port = graylogPort,
                        TransportType = Serilog.Sinks.Graylog.Core.Transport.TransportType.Udp
                    });
            }
            else
            {
                var template = hc.Configuration["Logging:FileOutputTemplate"];
                var logsDirectory = hc.Configuration["Logging:LogsDirectory"];

                if (string.IsNullOrEmpty(template) || string.IsNullOrEmpty(logsDirectory))
                {
                    return;
                }

                if (!string.IsNullOrEmpty(logsDirectory))
                {
                    if (!Directory.Exists(logsDirectory))
                    {
                        Directory.CreateDirectory(logsDirectory);
                    }
                }

                config.WriteTo.Logger(
                    x => x.Filter.ByIncludingOnly(l => l.Level < LogEventLevel.Error)
                        .WriteTo.File(
                            Path.Combine(logsDirectory, "log.txt"),
                            outputTemplate: template,
                            rollingInterval: RollingInterval.Hour));

                config.WriteTo.Logger(
                    x => x.Filter.ByIncludingOnly(l => l.Level == LogEventLevel.Error || l.Level == LogEventLevel.Fatal)
                        .WriteTo.File(
                            Path.Combine(logsDirectory, "log-error.txt"),
                            outputTemplate: template,
                            rollingInterval: RollingInterval.Hour));
            }
        }
    }
}