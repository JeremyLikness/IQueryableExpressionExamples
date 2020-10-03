// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System.Reflection;
using ExpressionPowerTools.Serialization.EFCore.AspNetCore.Extensions;
using ExpressionPowerTools.Serialization.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestDatabase;
using TestDatabase.SampleData;
using TestResultsBlazorApp.Shared;

namespace TestResultsBlazorApp.Server
{
    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The app configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// Configure services (dependency injection) for the app.
        /// </summary>
        /// <param name="services">The list of services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // set up our EF Core data context with SQLite.
            services.AddDbContext<TestDataContext>(opt => opt.UseSqlite("Data Source=testresults.db"));
            services.AddControllersWithViews();
            services.AddRazorPages();

            // custom for demo only
            CheckAndSeedDatabase(services);
        }

        /// <summary>
        /// Configure the web app.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/>.</param>
        /// <param name="env">The <see cref="IWebHostEnvironment"/>.</param>
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
                // use remote queries and allow special methods
                endpoints.MapPowerToolsEFCore<TestDataContext>(
                    rules: rules => rules.RuleForType<TestEntry>().Allow()
                        .RuleForMethod(selector => selector.ByResolver<MethodInfo, bool>(
                            (val) => DbFunctionsExtensions.Like(null, null, null))).Allow());
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
            });
        }

        /// <summary>
        /// Check to see if the database exists. If not, create it and seed it
        /// with the test data.
        /// </summary>
        /// <param name="services">The services.</param>
        private void CheckAndSeedDatabase(IServiceCollection services)
        {
            using var context = services.BuildServiceProvider().GetService<TestDataContext>();
            new SampleDataLoader().CheckAndSeed(context);
        }
    }
}
