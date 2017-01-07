using Microsoft.AspNetCore.Modules;

namespace Microsoft.AspNetCore.ServiceStack.Modules
{
    public static class ServiceCollectionBuilderExtensions
    {
        public static ModularServiceCollection AddServiceStackModules(this ModularServiceCollection moduleServices)
        {
            moduleServices.Configure(services =>
            {
                
            });

            return moduleServices;
        }
    }
}