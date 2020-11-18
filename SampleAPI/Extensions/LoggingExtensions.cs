using System;

using Microsoft.Extensions.Hosting;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;
using Serilog.Sinks.SystemConsole.Themes;

namespace SampleAPI.Extensions
{
    public static class LoggingExtensions
    {
        public static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration configuration)
        {
            var logConfig = configuration
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Filter.ByExcluding("RequestPath like '/metrics%'")
                .Filter.ByExcluding("RequestPath like '/swagger%'")
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Application", context.Configuration["ApplicationName"])
                .Enrich.WithProperty("source", Environment.MachineName);

            var consoleTemplate = context.Configuration["Logging:ConsoleOutputTemplate"];
            if (!string.IsNullOrEmpty(consoleTemplate))
            {
                logConfig.WriteTo.Console(
                    outputTemplate: consoleTemplate,
                    theme: AnsiConsoleTheme.Literate);
            }

            if (!bool.Parse(context.Configuration["Logging:Graylog:Enabled"]))
            {
                return;
            }

            var graylogHost = context.Configuration["Logging:Graylog:Host"];
            var graylogPort = int.Parse(context.Configuration["Logging:Graylog:Port"]);

            logConfig.WriteTo.Graylog(
                new GraylogSinkOptions
                    {
                        HostnameOrAddress = graylogHost,
                        Port = graylogPort,
                        TransportType = TransportType.Udp
                    });
        }
    }
}