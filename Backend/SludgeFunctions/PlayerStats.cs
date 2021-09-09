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
            data.Ip = Statics.GetIpFromRequestHeaders(req);

            string path = $"{Statics.VersionName}/RoundResults/{data.UniqueId}.json";

            await Statics.BlobStorage.StoreTextBlobAsync(path, content);
            Counters.AddAttemptForLevel();

            return new OkObjectResult("");
        }

        [FunctionName("get-round-result")]
        public static async Task<IActionResult> GetRoundResult(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "get-round-result/{uniqueId}")] HttpRequest req,
            string uniqueId)
        {
            string path = $"{Statics.VersionName}/RoundResults/{uniqueId}.json";
            string json = await Statics.BlobStorage.GetTextBlobAsync(path);
            return new OkObjectResult(json);
        }

        [FunctionName("world-wide-attempts")]
        public static IActionResult GetCount([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            long attempts = Counters.GetAttempts();
            return new OkObjectResult(attempts.ToString());
        }
    }
}
