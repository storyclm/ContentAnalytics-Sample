using System.Threading.Tasks;
using Breffi.Story.SDK.CLMAnalitycs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryCLM.SDK;
using Сonsumer.Options;

namespace Сonsumer.Synchronizers
{
    public class KpisSynchronizer : SynchronizerBase
    {
        public KpisSynchronizer(SCLM sclm, IOptionsSnapshot<SyncOptions> options, ILogger<KpisSynchronizer> logger) :
            base(sclm, options, logger)
        {
        }

        public override async Task RunAsync()
        {
            Logger.LogInformation("KPISs events synchronizer");
            await base.RunAsync();
        }

        internal override async Task FeedAsync()
        {
            var kpisEvent = await Sclm.GetKPIS(int.Parse(Options.PId), Options.UId, SetSections(Options.Sections));

            Logger.LogInformation(
                $"KPIS: {kpisEvent.Id}, Data: {kpisEvent.Date:dd.MM.yyyy HH:mm:ss}, PresentationId: {kpisEvent.PresentationId}, SlideId: {kpisEvent.SlideId}, User: {kpisEvent.UserId}");

            kpisEvent.SaveTo("Data/KPISs");
        }
    }
}
