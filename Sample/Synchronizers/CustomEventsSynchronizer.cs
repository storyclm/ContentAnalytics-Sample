using System;
using System.Threading.Tasks;
using Breffi.Story;
using Breffi.Story.SDK.CLMAnalitycs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryCLM.SDK;
using Сonsumer.Options;

namespace Сonsumer.Synchronizers
{
    public class CustomEventsSynchronizer : SynchronizerBase
    {
        public CustomEventsSynchronizer(SCLM sclm, IOptionsSnapshot<SyncOptions> options,
            ILogger<CustomEventsSynchronizer> logger) : base(sclm, options, logger)
        {
        }

        public override async Task RunAsync()
        {
            Logger.LogInformation("Custom events synchronizer");
            await base.RunAsync();
        }


        internal override async Task FeedAsync()
        {
            using var state = new ContinuationSync<long?>($"csstate-{Options.PId}");
            var feed = Sclm.GetCustomEventsFeed(Options.PId, Options.UId, SetSections(Options.Sections), state.Token); 

            foreach (var page in feed)
            {
                await page.Result.ThrottleAsync(customEvent =>
                {
                    Logger.LogInformation(
                        $"CUSTOM EVENT: {customEvent.Id}, Data: {new DateTime(customEvent.LocalTicks):dd.MM.yyyy HH:mm:ss}, PresentationId: {customEvent.PresentationId}, SessionId: {customEvent.SessionId}, User: {customEvent.UserId}");

                    customEvent.SaveTo("Data/CustomEvents"); 
                    return Task.CompletedTask;
                }, 10);
                state.Token = page.ContinuationToken;
            }
        }
    }
}
