using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StoryCLM.SDK;
using Сonsumer.Options;

namespace Сonsumer.Synchronizers
{
    public abstract class SynchronizerBase
    {
        internal readonly ILogger Logger;
        internal readonly SyncOptions Options;
        internal readonly SCLM Sclm;

        protected SynchronizerBase(SCLM sclm, IOptionsSnapshot<SyncOptions> options,
            ILogger logger)
        {
            Sclm = sclm ?? throw new ArgumentNullException(nameof(sclm));
            Options = options.Value ?? throw new ArgumentNullException(nameof(options));
            Logger = logger ?? throw new ArgumentException(nameof(logger));
        }

        public virtual async Task RunAsync()
        {
            while (true)
            {
                try
                {
                    await FeedAsync();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, ex.Message);
                }
                finally
                {
                    await Task.Delay(10_000);
                }
            }
        }

        /// <summary>
        ///     Потреблять события из ленты можно по секциям.
        ///     Курсор устанавливается в начало секции и ограничивается ее концом.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        internal Section SetSections(string section)
        {
            if (string.IsNullOrWhiteSpace(section))
            {
                return null;
            }

            return section switch
            {
                "h" => new HourSection().Hour(DateTimeOffset.UtcNow.Hour), // текущий час
                "d" => new DaySection().Day(DateTimeOffset.UtcNow.Day), // текущий день
                "m" => new MonthSection().Month(DateTimeOffset.UtcNow.Month), // текущий месяц
                "y" => new YearSection().Year(DateTimeOffset.UtcNow.Year), // текущий год
                _ => throw new ArgumentException(nameof(section))

                //Задает час в текущем дне
                //new HourSection().Hour(23);

                //задает день и час
                //new DaySection().Day(10).Hour(23);

                //задает месяц, день и час
                //new MonthSection().Month(5).Day(10).Hour(23);

                //задает год, месяц, день и час
                //new YearSection().Year(2018).Month(5).Day(10).Hour(23);

                //весь указанный день
                //new DaySection().Day(10);

                //весь указанный месяц
                //new MonthSection().Month(5);

                //весь указанный год
                //new YearSection().Year(2018);
            };
        }

        internal abstract Task FeedAsync();
    }
}
