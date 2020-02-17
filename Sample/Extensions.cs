using Breffi.Story.SDK.CLMAnalitycs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Сonsumer
{
    public static class Extensions
    {
        public static void SaveTo(this object o, string directory)
        {
            if (o == null) return;
            var oType = o.GetType();
            if (!oType.IsSubclassOf(typeof(StoryAnalitycsEvent))) return;
            var storyEvent = (StoryAnalitycsEvent)o;
            var date = new DateTime(storyEvent.LocalTicks);

            var path = Path.Combine(directory, 
                $"Presentation-{storyEvent.PresentationId}", 
                date.Year.ToString(), 
                date.ToString("MMMM"), 
                date.ToString("dd (dddd)"), 
                date.ToString("HH"));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            File.WriteAllText($"{path}/{storyEvent.SessionId}.json", JsonConvert.SerializeObject(o, Formatting.Indented));
        }
    }
}
