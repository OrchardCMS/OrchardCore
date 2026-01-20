using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.Mvc.RazorPages;

public static class ModularPageMvcCoreBuilderExtensions
{
    public static IMvcCoreBuilder AddModularRazorPages(this IMvcCoreBuilder builder)
    {
        builder.AddRazorPages();
        builder.Services.AddModularRazorPages();
        return builder;
    }

    internal static IServiceCollection AddModularRazorPages(this IServiceCollection services)
    {
        services.AddSingleton<MatcherPolicy, PageEndpointComparerPolicy>();

        services.AddTransient<IConfigureOptions<RazorPagesOptions>, ModularPageRazorPagesOptionsSetup>();

        services.AddSingleton<IPageApplicationModelProvider, ModularPageApplicationModelProvider>();

        return services;
    }
}
