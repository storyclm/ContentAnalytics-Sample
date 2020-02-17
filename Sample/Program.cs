using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryCLM.SDK;
using StoryCLM.SDK.Authentication;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Сonsumer
{
    class Program
    {
        static IConfigurationRoot Configuration;

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddLogging(s => s.AddConsole());

            services.AddOptions();
            services.Configure<StoryOptions>(Configuration.GetSection("Story"));
            services.Configure<SyncOptions>(Configuration.GetSection("Sync"));

            services.AddSingleton(s =>
            {
                var options = s.GetService<IOptionsSnapshot<StoryOptions>>().Value;
                SCLM sclm = new SCLM();
                sclm.SetEndpoint("content", options.AnalyticsEndpoint);
                sclm.SetEndpoint("auth", options.AuthEndpoint);
                var token = sclm.AuthAsync(options.Client, options.Secret).GetAwaiter().GetResult();
                return sclm;
            });

            services.AddTransient<SessionsSynchronizer>();
            return services;
        }

        static async Task Main(string[] args)
        {
            Configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();
            await serviceProvider.GetService<SessionsSynchronizer>().RunAsync();
        }
    }
}
