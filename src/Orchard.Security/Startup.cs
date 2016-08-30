using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Security
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAuthorization();
        }
    }
}
