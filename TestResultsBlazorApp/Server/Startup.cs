using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using TestDatabase;
using System;
using TestDatabase.SampleData;
using Microsoft.EntityFrameworkCore;
using ExpressionPowerTools.Serialization.EFCore.AspNetCore.Extensions;
using TestResultsBlazorApp.Shared;
using ExpressionPowerTools.Serialization.Extensions;
using System.Reflection;

namespace TestResultsBlazorApp.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TestDataContext>(opt => opt.UseSqlite("Data Source=testresults.db"));
            services.AddControllersWithViews();
            services.AddRazorPages();

            CheckAndSeedDatabase(services);
        }

        private void CheckAndSeedDatabase(IServiceCollection services)
        {
            using var context = services.BuildServiceProvider().GetService<TestDataContext>();
            new SampleDataLoader().CheckAndSeed(context);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPowerToolsEFCore<TestDataContext>(
                    rules: rules => rules.RuleForType<TestEntry>().Allow()
                        .RuleForMethod(selector => selector.ByResolver<MethodInfo, bool>(
                            (val) => DbFunctionsExtensions.Like(null, null, null))).Allow());
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }
    }
}
