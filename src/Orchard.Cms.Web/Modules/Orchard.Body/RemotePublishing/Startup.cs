using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Body.Drivers;

namespace Orchard.Body.RemotePublishing
{

    [OrchardFeature("Orchard.RemotePublishing")]
    public class RemotePublishingStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMetaWeblogDriver, BodyMetaWeblogDriver>();
        }
    }
}
