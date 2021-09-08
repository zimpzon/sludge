using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System.IO;
using Newtonsoft.Json;
using SludgeFunctions;

namespace Sludge
{
    public static class PlayerStats
    {
        [FunctionName("store-events")]
        public static async Task<IActionResult> StoreEvent([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            string content = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<RoundResult>(content);
            string path = $"{Statics.VersionName}/RoundResults/{data.UniqueId}.json";

            await Statics.BlobStorage.StoreTextBlobAsync(path, content);
            Counters.AddAttemptForLevel(data.LevelId);

            return new OkObjectResult("");
        }

        [FunctionName("world-wide-attempts")]
        public static IActionResult GetCount([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            long attempts = Counters.totalAttempts;
            return new OkObjectResult(attempts.ToString());
        }
    }
}
