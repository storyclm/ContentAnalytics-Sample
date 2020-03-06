using System;
using System.IO;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryCLM.SDK;
using StoryCLM.SDK.Authentication;
using Сonsumer.Options;
using Сonsumer.Synchronizers;

namespace Сonsumer
{
    public class Program
    {
        private static IConfigurationRoot _configuration;

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            services.AddLogging(s => s.AddConsole());

            services.AddOptions();
            services.Configure<StoryOptions>(_configuration.GetSection("Story"));
            services.Configure<SyncOptions>(_configuration.GetSection("Sync"));

            services.AddSingleton(s =>
            {
                var options = s.GetService<IOptionsSnapshot<StoryOptions>>().Value;
                var sclm = new SCLM();
                sclm.SetEndpoint("content", options.AnalyticsEndpoint);
                sclm.SetEndpoint("auth", options.AuthEndpoint);
                sclm.AuthAsync(options.Client, options.Secret).GetAwaiter().GetResult();
                return sclm;
            });

            services.AddTransient<SessionsSynchronizer>();
            services.AddTransient<SlidesSynchronizer>();
            return services;
        }

        public static async Task Main(string[] args)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(AppContext.BaseDirectory))
                .AddJsonFile("appsettings.json", true)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            var syncOptions = serviceProvider.GetService<IOptionsSnapshot<SyncOptions>>().Value;
            SynchronizerBase synchronizer = syncOptions.Mode switch
            {
                "sessions" => serviceProvider.GetService<SessionsSynchronizer>(),
                "slides" => serviceProvider.GetService<SlidesSynchronizer>(),
                _ => throw new ArgumentException(nameof(syncOptions.Mode))
            };
            await synchronizer.RunAsync();
        }
    }
}
