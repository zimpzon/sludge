namespace Sludge
{
    public static class Statics
    {
        public static readonly string VersionName = "webgl-beta1";

        public static IBlobStorage BlobStorage = new BlobStorage(Startup.Config);
    }
}
