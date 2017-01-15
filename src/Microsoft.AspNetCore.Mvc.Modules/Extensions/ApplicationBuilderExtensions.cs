using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public static class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseMvcModules(this IApplicationBuilder app, Action<ModularApplicationBuilder> modules)
        {
            var modularApplicationBuilder = new ModularApplicationBuilder(app);
            modules(modularApplicationBuilder);

            return app;
        }
    }
}