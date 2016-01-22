using Orchard.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Security
{
    public class AuthorizationServiceModule : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddAuthorization();
        }
    }
}
