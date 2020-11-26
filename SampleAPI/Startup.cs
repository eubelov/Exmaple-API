using System;
using System.IO;

using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Prometheus;

using SampleAPI.Auth;
using SampleAPI.Extensions;
using SampleAPI.Filters;

using Serilog;

namespace SampleAPI
{
    public class Startup
    {
        private IConfiguration configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            this.SetupConfiguration();

            services.AddControllers(
                options =>
                    {
                        options.UseGlobalRoutePrefix("api/v{version:apiVersion}");
                        options.Filters.Add(new HttpResponseExceptionFilter());
                    });

            services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                });

            services.AddApiVersioning(options =>
                {
                    options.ReportApiVersions = false;
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = new ApiVersion(1, 0);
                });

            services.AddPolly(this.configuration);
            services.AddJwt(this.configuration);
            services.AddAutoMapper(typeof(Startup));
            services.AddHttpContextAccessor();
            services.AddMediatR(typeof(Startup));
            services.AddOptions();
            services.AddSwagger();

            services.AddCors(
                options =>
                    {
                        options.AddPolicy(
                            "AllowAll",
                            builder =>
                                {
                                    builder.AllowAnyMethod()
                                        .AllowAnyHeader()
                                        .SetIsOriginAllowed(host => true)
                                        .AllowCredentials();
                                });
                    });

            services.AddAuthorization(
                config =>
                    {
                        config.AddPolicy(Policies.Admin, Policies.AdminPolicy());
                        config.AddPolicy(Policies.AuthenticatedUser, Policies.AnyAuthenticatedUserPolicy());
                        config.AddPolicy(Policies.User, Policies.UserPolicy());
                    });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseSerilogRequestLogging();
            app.UseForwardedHeaders();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMetricServer();
            app.UseHttpMetrics();
            app.UseCors("AllowAll");

            if (!env.IsProduction())
            {
                app.UseSwagger()
                    .UseSwaggerUI(
                        c =>
                            {
                                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Sample API");
                            });
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                    {
                        endpoints.MapControllers();
                        endpoints.MapMetrics();
                        endpoints.MapHealthChecks("/hc");
                    });
        }

        private void SetupConfiguration()
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddJsonFile($"appsettings.{environmentName}.json", true, true)
                .AddEnvironmentVariables();

            var config = builder.Build();

            if (config.GetValue("UseVault", false))
            {
                builder.AddAzureKeyVault(config["KeyVault:Url"]);
            }

            this.configuration = builder.Build();
        }
    }
}