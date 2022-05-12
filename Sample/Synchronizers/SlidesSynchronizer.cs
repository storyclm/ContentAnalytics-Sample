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
    public class SlidesSynchronizer : SynchronizerBase
    {
        public SlidesSynchronizer(SCLM sclm, IOptionsSnapshot<SyncOptions> options,
            ILogger<SlidesSynchronizer> logger) : base(sclm, options, logger)
        {
        }

        public override async Task RunAsync()
        {
            Logger.LogInformation("Slides synchronizer");
            await base.RunAsync();
        }


        internal override async Task FeedAsync()
        {
            using var state = new ContinuationSync<long?>($"slidesstate-{Options.PId}");
            var feed = Sclm.GetSlidesFeed(Options.PId, Options.UId, SetSections(Options.Sections), state.Token); 

            foreach (var page in feed)
            {
                await page.Result.ThrottleAsync(slide =>
                {
                    Logger.LogInformation(
                        $"SLIDE: {slide.SlideId}, SlideName: {slide.SlideName}, Data: {new DateTime(slide.LocalTicks):dd.MM.yyyy HH:mm:ss}, Duration: {slide.Duration}, SessionId: {slide.SessionId}, User: {slide.UserId}, Presentation: {slide.PresentationId}");

                    slide.SaveTo("Data/Slides"); 
                    return Task.CompletedTask;
                }, 10);
                state.Token = page.ContinuationToken;
            }
        }
    }
}
