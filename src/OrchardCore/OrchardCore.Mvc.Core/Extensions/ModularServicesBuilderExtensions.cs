using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;

namespace OrchardCore.Mvc
{
    public static class ModularServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level MVC services.
        /// </summary>
        public static ModularServicesBuilder AddMvc(this ModularServicesBuilder builder)
        {
            builder.Services.ConfigureTenantServices<IServiceProvider>((collection, sp) =>
            {
                collection.AddMvcModules(sp);
            })
            .ConfigureTenant((app, routes, sp) =>
            {
                app.UseStaticFilesModules();
            });

            return builder;
        }

        /// <summary>
        /// Adds tenant level tag helper services.
        /// </summary>
        public static ModularServicesBuilder AddTagHelpers(this ModularServicesBuilder builder, string assemblyName)
        {
            builder.Services.ConfigureTenant((app, routes, sp) =>
            {
                sp.AddTagHelpers(assemblyName);
            });

            return builder;
        }
    }
}
