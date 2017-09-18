using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NADAAPI.Data;
using NADAAPI.Repository;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.IdentityModel.Tokens;
using NADAAPI.Authorization;
using NADAAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace NADAAPI
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton(Configuration);
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);

            services.AddDbContext<NADAAPIDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("NADAAPIDB")));

            services.Configure<ImpexiumProperties>(Configuration.GetSection("ImpexiumProperties"));
            services.Configure<JWToken>(Configuration.GetSection("Tokens"));
            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<NADAAPIDbContext>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
            services.AddSingleton<IAuthorizationHandler, VendorHandler>();
            services.AddSession();
           
            services.AddAuthorization(options =>
            {
                options.AddPolicy("VendorCheck", v => v.Requirements.Add(new VendorRequirement()));
            });

            services.AddLogging();
            services.AddMvc();
            services.AddTransient<IVendorRepository, VendorRepository>();
            services.AddTransient<IImpexiumRepository, ImpexiumRepository>();
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
 
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
               
            }
            else
            {
                app.UseExceptionHandler();
            }

            app.UseStaticFiles();
            app.UseStatusCodePages(); //To return correct status codes instead of empty page
            app.UseIdentity();
            app.UseSession();
            app.UseJwtBearerAuthentication(new JwtBearerOptions()
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = Configuration["Tokens:Issuer"],
                    ValidAudience = Configuration["Tokens:Audience"],
                  
            ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))

                }
            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Vendor}/{action=List}/{id?}");
            }); ;
        }
    }
}
