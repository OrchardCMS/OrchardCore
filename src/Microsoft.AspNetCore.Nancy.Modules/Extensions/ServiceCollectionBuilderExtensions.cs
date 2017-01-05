using Microsoft.AspNetCore.Modules;

namespace Microsoft.AspNetCore.Nancy.Modules
{
    public static class ServiceCollectionBuilderExtensions
    {
        public static ModularServiceCollection AddNancyModules(this ModularServiceCollection moduleServices)
        {
            moduleServices.Configure(services =>
            {
                
            });

            return moduleServices;
        }
    }
}