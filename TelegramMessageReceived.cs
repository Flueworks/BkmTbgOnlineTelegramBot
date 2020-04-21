using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Telegram.Bot.Types;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Configuration;
using System.Reflection;

namespace Bkm.Online
{
    public static class TelegramMessageReceived
    {
        [FunctionName("TelegramMessageReceived")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            IBinder binder,
            ILogger log)
        {
            var json = await req.ReadAsStringAsync();

            var update = JsonConvert.DeserializeObject<Update>(json);

            if(update.Type == UpdateType.Message)
            {
                var message = update.Message;
                if(message.Photo != null && message.Photo.Any())
                {
                    var data = await DownloadPhotos(message.Photo);

                }
            }

            return new OkResult();
        }

        private static async Task<byte[]> DownloadPhotos(PhotoSize[] photos)
        {
            var photo = photos.OrderByDescending(x=>x.FileSize).FirstOrDefault();

            var config = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddUserSecrets(Assembly.GetExecutingAssembly(), true)
                    .Build();
            var bot = new TelegramBotClient(config["apikey"]);

            var file = await bot.GetFileAsync(photo.FileId);
            using MemoryStream ms = new MemoryStream();
            await bot.DownloadFileAsync(file.FilePath, ms);

            return ms.ToArray();
        }
    }

}
