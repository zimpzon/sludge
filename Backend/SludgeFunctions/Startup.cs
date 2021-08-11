using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Sludge.Startup))]

namespace Sludge
{
    public class Startup : FunctionsStartup
    {
        public static IConfiguration Config = null;

        public override void Configure(IFunctionsHostBuilder builder)
        {
            Config = builder.GetContext().Configuration;
        }
    }
}