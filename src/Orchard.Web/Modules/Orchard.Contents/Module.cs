using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.DependencyInjection;

namespace Orchard.Contents
{
    public class Moduke : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddContentManagement();
        }
    }
}