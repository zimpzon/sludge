using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace Sludge
{
    public static class PlayerStats
    {
        [FunctionName("store-events")]
        public static async Task<IActionResult> StoreEvent([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req)
        {
            string content = await new StreamReader(req.Body).ReadToEndAsync();
            await Statics.BlobStorage.StoreTextBlobAsync($"webgl-beta1/{DateTime.UtcNow:O}.json", content);

            return new OkObjectResult("");
        }
    }
}
