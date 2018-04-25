using System;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OrchardCore.Mvc.RazorPages
{
    public static class ModularPageMvcCoreBuilderExtensions
    {
        public static IMvcCoreBuilder AddModularRazorPages(this IMvcCoreBuilder builder, IServiceProvider services)
        {
            builder.AddRazorPages();

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorPagesOptions>, ModularPageRazorPagesOptionsSetup>());

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Transient<IConfigureOptions<RazorViewEngineOptions>, ModularPageRazorViewEngineOptionsSetup>());

            builder.Services.TryAddEnumerable(
                ServiceDescriptor.Singleton<IPageApplicationModelProvider, ModularPageApplicationModelProvider>());

            return builder;
        }
    }
}
