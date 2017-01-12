using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Orchard.Environment.Extensions;

namespace Microsoft.AspNetCore.Mvc.Modules
{
    public static class ApplicationBuilderExtensions
    {
        public static ModularApplicationBuilder UseMvcModules(this ModularApplicationBuilder modularApp)
        {
            modularApp.Configure(app =>
            {
                app.UseMvcModules();
            });

            return modularApp;
        }

        public static IApplicationBuilder UseMvcModules(this IApplicationBuilder builder)
        {
            return builder;
        }
    }
}