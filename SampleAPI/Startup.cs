using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

using AutoMapper;

using MediatR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Newtonsoft.Json;

using Polly;
using Polly.Registry;

using Prometheus;

using SampleAPI.Auth;

namespace SampleAPI
{
    public class Startup
    {
        private IConfiguration configuration;

        public void ConfigureServices(IServiceCollection services)
        {
            this.SetupConfiguration();
            this.SetupPolly(services);

            services.AddControllers();

            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddJwtBearer(
                    "Bearer",
                    options =>
                        {
                            options.Authority = this.configuration["JwtIssuerOptions:Issuer"];
                            options.Audience = this.configuration["JwtIssuerOptions:Audience"];
                            options.RequireHttpsMetadata = false;
                            options.SaveToken = true;
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = this.configuration["JwtIssuerOptions:Issuer"],
                                ValidAudience = this.configuration["JwtIssuerOptions:Audience"],
                                ClockSkew = TimeSpan.Zero
                            };

                            options.Events = new JwtBearerEvents
                            {
                                OnAuthenticationFailed = context =>
                                {
                                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                                    {
                                        context.Response.Headers.Add("Token-Expired", "true");
                                    }

                                    return Task.CompletedTask;
                                }
                            };
                        });

            services.AddAutoMapper(typeof(Startup));
            services.AddHttpContextAccessor();
            services.AddMediatR(typeof(Startup));
            services.AddOptions();

            services.AddSwaggerGen(
                options =>
                    {
                        options.AddSecurityDefinition(
                            "Bearer",
                            new OpenApiSecurityScheme
                            {
                                In = ParameterLocation.Header,
                                Description = "Please insert JWT with Bearer into field",
                                Name = "Authorization",
                                Type = SecuritySchemeType.ApiKey
                            });

                        options.SwaggerDoc(
                            "v1",
                            new OpenApiInfo
                            {
                                Title = "Sample API",
                                Version = "v1",
                                Description = "An API to perform User and Organization operations",
                                TermsOfService = new Uri("https://example.com/terms"),
                                Contact = new OpenApiContact
                                {
                                    Name = "Evgeny Belov",
                                    Email = "ev@ev.com",
                                    Url = new Uri("https://github.com/eubelov"),
                                },
                                License = new OpenApiLicense
                                {
                                    Name = "User and Organization API LICX",
                                    Url = new Uri("https://example.com/license"),
                                }
                            });

                        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                        options.IncludeXmlComments(xmlPath);
                    });

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

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var errorLogger = loggerFactory.CreateLogger("GlobalExceptionHandler");
            app.UseMetricServer();
            app.UseHttpMetrics();

            app.UseExceptionHandler(
                appError =>
                    {
                        appError.Run(
                            async context =>
                                {
                                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                    context.Response.ContentType = "application/json";

                                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                                    if (contextFeature != null)
                                    {
                                        errorLogger.LogError(contextFeature.Error, "Internal error happened");

                                        object message;
                                        if (env.IsDevelopment())
                                        {
                                            message = new
                                            {
                                                StatusCode = (int)HttpStatusCode.InternalServerError,
                                                Message = contextFeature.Error
                                            };
                                        }
                                        else
                                        {
                                            message = new
                                            {
                                                StatusCode = (int)HttpStatusCode.InternalServerError,
                                                Message = "Internal Server Error"
                                            };
                                        }

                                        await context.Response.WriteAsync(JsonConvert.SerializeObject(message));
                                    }
                                });
                    });

            var pathBase = this.configuration["PATH_BASE"];
            if (!string.IsNullOrEmpty(pathBase))
            {
                loggerFactory.CreateLogger<Startup>().LogDebug("Using PATH BASE '{pathBase}'", pathBase);
                app.UsePathBase(pathBase);
            }

            app.UseCors("AllowAll");

            if (!env.IsProduction())
            {
                app.UseSwagger()
                    .UseSwaggerUI(
                        c =>
                            {
                                c.SwaggerEndpoint(
                                    $"{(string.IsNullOrEmpty(pathBase) ? string.Empty : pathBase)}/swagger/v1/swagger.json",
                                    "Sample API");
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
                    });
        }

        private void SetupPolly(IServiceCollection services)
        {
            var registry = new PolicyRegistry();
            var retriesCount = this.configuration.GetValue<int>("RetriesCount");

            services.AddSingleton<IReadOnlyPolicyRegistry<string>>(
                x =>
                    {
                        var logger = x.GetRequiredService<ILogger<Startup>>();

                        var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(
                            retriesCount,
                            retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                            (exception, timeSpan) =>
                                {
                                    logger.LogError(exception, $"Operation failed, retrying in {timeSpan.Seconds} seconds");
                                });

                        registry.Add("defaultRetryPolicy", retryPolicy);

                        return registry;
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