using Microsoft.Extensions.DependencyInjection.Extensions;
using OrchardCore.Localization.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class SharedDataLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddSharedDataLocalizationProvider<TSharedLocalizationDataProvider>(this IServiceCollection services)
        where TSharedLocalizationDataProvider : class, ISharedLocalizationDataProvider
    {
        services.AddSingleton<ISharedLocalizationDataProvider, TSharedLocalizationDataProvider>();
        services.TryAddSingleton<ILocalizationDataProvider, SharedLocalizationDataProvider>();

        return services;
    }
}
