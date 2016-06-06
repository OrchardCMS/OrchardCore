using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;
using Orchard.Lists.Indexes;

namespace Orchard.Lists
{
    public class Module : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<ContainedPartIndexProvider>();
        }
    }
}
