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
    public class SessionsSynchronizer : SynchronizerBase
    {
        public SessionsSynchronizer(SCLM sclm, IOptionsSnapshot<SyncOptions> options,
            ILogger<SessionsSynchronizer> logger) : base(sclm, options, logger)
        {
        }

        public override async Task RunAsync()
        {
            Logger.LogInformation("Sessions synchronizer");
            await base.RunAsync();
        }


        internal override async Task FeedAsync()
        {
            using (var state = new ContinuationSync<long?>($"sessionsstate-{Options.PId}"))
            {
                //Получение ленты событий. Если не задать курсор (continuationToken) то потребелние событий будет начинаться с перого события.
                var feed = Sclm.GetSessionFeed(Options.PId, //Указываем идентификатор презентации
                    Options.UId, // если не null то будет отфильтровано по пользователю, иначе все события ленты этой презентации
                    SetSections(Options.Sections),
                    state.Token); // токен продолжения, позиция курсора в ленте.

                //События ленты потребляются постранично. При получении страницы вместе с ней приходит continuationToken. По этому токену можно получить слудющую страницу. 
                //Сохраняя этот токен, возможно прожолжить обход ленты в следующий раз если в данный момент в ленте 0 событий или произошел краш.
                foreach (var page in feed)
                {
                    //Обработка страницы параллельно с ограничением в 10 потоков
                    await page.Result.ThrottleAsync(async session =>
                    {
                        Logger.LogInformation(
                            $"SESSION: {session.SessionId}, Data: {new DateTime(session.LocalTicks):dd.MM.yyyy HH:mm:ss}, Slides: {session.SlidesCount}, Duration: {session.Duration}, User: {session.UserId}, Presentation: {session.PresentationId}, Address: {session.Address}");

                        //Получение визита по идентификатору сессии. Визит включает: сессию, список слайдов и собранные в один объект кастомные события.
                        var visit = await Sclm.GetVisit<PresentationData>(session.SessionId);
                        var slides = visit.Slides; // стэк показа слайдов
                        var data = visit.CustomEvents; // данные из презентации агрегированные в один обхект

                        visit.SaveTo("Data/Sessions"); // сохранение в хранилище)) 
                    }, 1); // сколько одновременно обрабатывать
                    state.Token = page.ContinuationToken; // сохранение токена
                }
            }
        }
    }
}
