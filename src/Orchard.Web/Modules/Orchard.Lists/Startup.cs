using Microsoft.Extensions.DependencyInjection;
using Orchard.Lists.Indexes;

namespace Orchard.Lists
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ContainedPartIndexProvider>();
        }
    }
}
