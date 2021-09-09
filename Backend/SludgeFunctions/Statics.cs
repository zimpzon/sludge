using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Sludge
{
    public static class Statics
    {
        public static readonly string VersionName = "webgl-beta1";

        public static IBlobStorage BlobStorage = new BlobStorage(Startup.Config);

        public static string GetIpFromRequestHeaders(HttpRequest request)
        {
            return (request.Headers["X-Forwarded-For"].FirstOrDefault() ?? "").Split(new char[] { ':' }).FirstOrDefault();
        }
    }
}
