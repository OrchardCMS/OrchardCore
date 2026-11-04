using Fluid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OrchardCore.Liquid;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the <see cref="IOptions{TemplateOptions}"/> of the tenant, which are built once from the
    /// <see cref="TemplateOptionsBuilder"/> configured with <c>services.Configure&lt;TemplateOptionsBuilder&gt;()</c>,
    /// and which only expose the members that are registered, see <see cref="AllowListMemberAccessStrategy"/>.
    /// </summary>
    public static IServiceCollection AddLiquidTemplateOptions(this IServiceCollection services)
    {
        services.Configure<TemplateOptionsBuilder>(o => o.WithMemberAccessStrategy<AllowListMemberAccessStrategy>());
        services.TryAddSingleton<IOptions<TemplateOptions>, TemplateOptionsAccessor>();

        return services;
    }

    public static IServiceCollection AddLiquidFilter<TLiquidFilter>(this IServiceCollection services, string name) where TLiquidFilter : class, ILiquidFilter
    {
        services.Configure<TemplateOptionsBuilder>(o => o.AddFilter(name, new LiquidFilterDelegateResolver<TLiquidFilter>().ResolveAsync));
        services.AddScoped<TLiquidFilter>();

        return services;
    }
}
