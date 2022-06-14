using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace OrchardCore.Testing.Context
{
    public static class SiteContextConfig
    {
        public static Type WebStartupClass { get; set; }

        public static RecipeLocator[] Recipies { get; set; }

        public static string[] TenantFeatures { get; set; }

        public static string[] AdditionalSetupFeatures { get; set; }

        public static IEnumerable<PermissionsContext> PermissionsContexts { get; set; }

        public static Action<IHostBuilder> ConfigureHostBuilder;

        public static Action<IApplicationBuilder> ConfigureAppBuilder;

        public static Action<IServiceCollection> ConfigureOrchardServices;

    }
}
