using Breffi.Story;
using Breffi.Story.SDK.CLMAnalitycs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StoryCLM.SDK;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Сonsumer
{
    public class SessionsSynchronizer
    {

        private readonly SCLM _sclm;
        private readonly SyncOptions _options;
        private readonly ILogger _logger;

        public SessionsSynchronizer(SCLM sclm, 
            IOptionsSnapshot<SyncOptions> options,
            ILogger<SessionsSynchronizer> logger)
        {
            _sclm = sclm ?? throw new ArgumentNullException(nameof(sclm));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentException(nameof(logger));
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Sessions synchronizer");
            while (true)
            {
                try
                {
                    await FeedAsync();
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, ex.Message);
                }
                finally
                {
                    await Task.Delay(10_000);
                }
            }
        }


        public async Task FeedAsync() 
        {
            using (var state = new ContinuationSync<long?>($"sessionsstate-{_options.PId}"))
            {
                //Получение ленты событий. Если не задать курсор (continuationToken) то потребелние событий будет начинаться с перого события.
                var feed = _sclm.GetSessionFeed(_options.PId, //Указываем идентификатор презентации
                    userId: _options.UId, // если не null то будет отфильтровано по пользователю, иначе все события ленты этой презентации
                    continuationToken: state.Token); // токен продолжения, позиция курсора в ленте.

                //События ленты потребляются постранично. При получении страницы вместе с ней приходит continuationToken. По этому токену можно получить слудющую страницу. 
                //Сохраняя этот токен, возможно прожолжить обход ленты в следующий раз если в данный момент в ленте 0 событий или произошел краш.
                foreach (var page in feed)
                {
                    //Обработка страницы параллельно с ограничением в 10 потоков
                    await page.Result.ThrottleAsync(async session =>
                    {
                        _logger.LogInformation($"SESSION: {session.SessionId}, Data: {new DateTime(session.LocalTicks):dd.MM.yyyy HH:mm:ss}, Slides: {session.SlidesCount}, Duration: {session.Duration}, User: {session.UserId}, Presentation: {session.PresentationId}, Address: {session.Address}");

                        //Получение визита по идентификатору сессии. Визит включает: сессию, список слайдов и собранные в один объект кастомные события.
                        var visit = await _sclm.GetVisit<PresentationData>(session.SessionId);
                        var slides = visit.Slides; // стэк показа слайдов
                        PresentationData data = visit.CustomEvents; // данные из презентации агрегированные в один обхект

                        visit.SaveTo("Data/Sessions"); // сохранение в хранилище)) 
                    }, 10);
                    state.Token = page.ContinuationToken; // сохранение токена
                }
            }
        }

        /// <summary>
        /// Потреблять события из ленты можно по секциям.
        /// Курсор устанавливается в начало секции и ограничивается ее концом.
        /// </summary>
        /// <param name="section"></param>
        /// <returns></returns>
        public Section SetSections(string section) => section switch
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
}
