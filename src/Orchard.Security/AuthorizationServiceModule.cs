using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Security
{
    public class AuthorizationServiceModule : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAuthorization();
        }
    }
}
