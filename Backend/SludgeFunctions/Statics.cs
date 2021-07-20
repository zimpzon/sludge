namespace Sludge
{
    public static class Statics
    {
        public static IBlobStorage BlobStorage = new BlobStorage(Startup.Config);
    }
}
