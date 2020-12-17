using Identity.API.Configuration;
using Identity.API.DataAccess;
using Identity.API.Extensions;
using Identity.API.Jwt;
using Identity.API.Models;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

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
            services.AddControllers().AddControllersAsServices();

            services.Configure<JwtSettings>(this.Configuration.GetSection(nameof(JwtSettings)));

            services.AddJwt(this.Configuration);
            services.AddSwagger();
            ServiceCollectionExtensions.RegisterMappings();

            services.AddCors(
                options =>
                    {
                        options.AddPolicy(
                            "AllowAll",
                            builder =>
                                {
                                    builder.AllowAnyOrigin()
                                        .AllowAnyMethod()
                                        .AllowAnyHeader();
                                });
                    });

            services.AddMvc();

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseSqlServer(this.Configuration.GetConnectionString("RentMe")));

            services.AddIdentity<ApplicationUser, IdentityRole>(
                    opt =>
                        {
                            opt.Password.RequireDigit = false;
                            opt.Password.RequireLowercase = false;
                            opt.Password.RequireNonAlphanumeric = false;
                            opt.Password.RequireUppercase = false;
                        })
                .AddEntityFrameworkStores<ApplicationDbContext>();

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

            app.UseSwagger()
                .UseSwaggerUI(
                    c =>
                        {
                            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TradeMe Auth API");
                        });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}