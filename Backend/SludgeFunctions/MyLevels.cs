using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Sludge.Shared;

namespace Sludge
{
    public static class Levels
    {
        const string MyLevelsPath = "mylevels";

        [FunctionName("mylevels")]
        public static async Task<IActionResult> MyLevels([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            string token = req.Query["token"];
            string userId = token;
            string myLevelsPath = $"{MyLevelsPath}/{userId}";
            var levelBlobs = await Statics.BlobStorage.GetBlobPaths(myLevelsPath);

            var result = new List<LevelData>();
            foreach(var level in levelBlobs)
            {

            }

            return new OkObjectResult("");
        }

        [FunctionName("createlevel")]
        public static async Task<IActionResult> Createlevel([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req, ILogger log)
        {
            string token = req.Query["token"];
            string userId = req.Query["userId"];

            string myLevelsPath = $"{MyLevelsPath}/{userId}";
            var levelBlobs = await Statics.BlobStorage.GetBlobPaths(myLevelsPath);
            foreach (var level in levelBlobs)
            {

            }

            return new OkObjectResult("");
        }
    }
}
