using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Polly;
using Polly.Registry;

namespace SampleAPI.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddJwt(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddAuthentication(
                    options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                .AddJwtBearer(
                    "Bearer",
                    options =>
                        {
                            options.Authority = configuration["JwtIssuerOptions:Issuer"];
                            options.Audience = configuration["JwtIssuerOptions:Audience"];
                            options.RequireHttpsMetadata = false;
                            options.SaveToken = true;
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = configuration["JwtIssuerOptions:Issuer"],
                                ValidAudience = configuration["JwtIssuerOptions:Audience"],
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

            return services;
        }

        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
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

            return services;
        }

        public static IServiceCollection AddPolly(this IServiceCollection services, IConfiguration configuration)
        {
            var registry = new PolicyRegistry();
            var retriesCount = configuration.GetValue<int>("RetriesCount");

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

                        registry.Add("defaultAsyncRetryPolicy", retryPolicy);

                        return registry;
                    });

            return services;
        }
    }
}