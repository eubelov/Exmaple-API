using System;
using System.Threading.Tasks;

using Identity.API.Configuration;
using Identity.API.Jwt;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace Identity.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging();
            services.AddControllers();

            services.AddTransient<AuthService>();
            services.Configure<Auth0Options>(this.Configuration.GetSection(nameof(Auth0Options)));

            services.AddAuthentication(
                    options =>
                        {
                            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        })
                .AddJwtBearer(
                    options =>
                        {
                            options.Authority = this.Configuration["JwtIssuerOptions:Issuer"];
                            options.Audience = this.Configuration["JwtIssuerOptions:Audience"];
                            options.RequireHttpsMetadata = false;
                            options.SaveToken = true;
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = this.Configuration["JwtIssuerOptions:Issuer"],
                                ValidAudience = this.Configuration["JwtIssuerOptions:Audience"],
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

            services.AddMvc();
            services.AddHttpClient(
                "auth0",
                x =>
                    {
                        x.BaseAddress = new Uri("https://evbelov.au.auth0.com/oauth/token");
                    });

            services.AddAuthorization(
                config =>
                    {
                        config.AddPolicy(Policies.Admin, Policies.AdminPolicy());
                        config.AddPolicy(Policies.User, Policies.UserPolicy());
                    });

            services.AddHealthChecks()
                .AddCheck("self", () => HealthCheckResult.Healthy());
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}