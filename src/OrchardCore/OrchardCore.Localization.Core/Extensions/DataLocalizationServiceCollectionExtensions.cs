using OrchardCore.Localization.Data;

namespace Microsoft.Extensions.DependencyInjection;

public static class DataLocalizationServiceCollectionExtensions
{
    public static IServiceCollection AddSharedDataLocalizationStrings(this IServiceCollection services, params string[] names)
    {
        ArgumentNullException.ThrowIfNull(names);

        var data = new HashSet<string>();
        foreach (var name in names)
        {
            data.Add(name);
        }

        services.AddSingleton<ILocalizationDataProvider>(new SharedLocalizationDataProvider(data));

        return services;
    }
}
