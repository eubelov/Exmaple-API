using System;
using System.Threading.Tasks;

using Identity.API.Models;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Nelibur.ObjectMapper;

namespace Identity.API.Extensions
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
                                Title = "RentMe Auth API",
                                Version = "v1",
                                TermsOfService = new Uri("https://example.com/terms"),
                                Contact = new OpenApiContact
                                {
                                    Name = "Evgeny Belov",
                                    Email = "ev@ev.com",
                                    Url = new Uri("https://github.com/eubelov"),
                                },
                                License = new OpenApiLicense { Url = new Uri("https://example.com/license"), }
                            });
                    });

            return services;
        }

        public static void RegisterMappings()
        {
            TinyMapper.Bind<ApplicationUser, UserViewModel>();
        }
    }
}