// Copyright (c) Jeremy Likness. All rights reserved.
// Licensed under the MIT License. See LICENSE in the repository root for license information.

using System;
using System.Net.Http;
using System.Threading.Tasks;
using ExpressionPowerTools.Serialization.EFCore.Http.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace TestResultsBlazorApp.Client
{
    /// <summary>
    /// Main app.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main entry for the app.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>An asynchronous <see cref="Task"/>.</returns>
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            // add our test service
            builder.Services.AddSingleton<TestDataAccess>();
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

            // add connection to server for remote query capabilities
            builder.Services.AddExpressionPowerToolsEFCore(new Uri(builder.HostEnvironment.BaseAddress));

            await builder.Build().RunAsync();
        }
    }
}
