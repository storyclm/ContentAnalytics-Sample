﻿using System.Threading.Tasks;
using Breffi.Story.SDK.CLMAnalitycs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryCLM.SDK;
using Сonsumer.Options;

namespace Сonsumer.Synchronizers
{
    public class KpipSynchronizer : SynchronizerBase
    {
        public KpipSynchronizer(SCLM sclm, IOptionsSnapshot<SyncOptions> options, ILogger<KpipSynchronizer> logger) :
            base(sclm, options, logger)
        {
        }

        public override async Task RunAsync()
        {
            Logger.LogInformation("KPIPs events synchronizer");
            await base.RunAsync();
        }

        internal override async Task FeedAsync()
        {
            var kpipEvent = await Sclm.GetKPIP(int.Parse(Options.PId), Options.UId, SetSections(Options.Sections));

            Logger.LogInformation(
                $"KPIP: {kpipEvent.Id}, Data: {kpipEvent.Date:dd.MM.yyyy HH:mm:ss}, PresentationId: {kpipEvent.PresentationId}, User: {kpipEvent.UserId}");

            kpipEvent.SaveTo("Data/KPIPs");
        }
    }
}
