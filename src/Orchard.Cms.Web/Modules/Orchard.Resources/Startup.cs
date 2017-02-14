using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ResourceManagement;

namespace Orchard.Resources
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection serviceCollection)
        {
            //var a1 = Assembly.Load(new AssemblyName("Orchard.DisplayManagement"));
            //partManager.ApplicationParts.Add(new AssemblyPart(a1));
            //var a2 = Assembly.Load(new AssemblyName("Orchard.Resources"));
            //partManager.ApplicationParts.Add(new AssemblyPart(a2));


            serviceCollection.AddScoped<IResourceManifestProvider, ResourceManifest>();
        }
    }
}
