using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Shortcodes.ViewModels;
using YesSql.Filters.Enumerable;

namespace OrchardCore.Shortcodes.Services;

public class DefaultShortcodeFilterParser(IEnumerableParser<DataSourceEntry> _parser)
    : IShortcodeFilterParser
{
    public EnumerableFilterResult<DataSourceEntry> Parse(string text) => _parser.Parse(text);
}

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShortcodeFilterParser(this IServiceCollection services) =>
        services.AddSingleton<IShortcodeFilterParser>(sp =>
        {
            var filterProviders = sp.GetServices<IShortcodeFilterProvider>().OrderBy(x => x.Order);
            var builder = new EnumerableEngineBuilder<DataSourceEntry>();
            foreach (var provider in filterProviders)
            {
                provider.Build(builder);
            }

            var parser = builder.Build();

            return new DefaultShortcodeFilterParser(parser);
        });
}
