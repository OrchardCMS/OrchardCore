using Microsoft.Extensions.Logging;
using OrchardCore.Admin.Models;
using OrchardCore.Modules;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListColumnsBuilder : IAdminListColumnsBuilder
{
    private readonly IEnumerable<IAdminListColumnsProvider> _providers;
    private readonly ILogger _logger;

    public DefaultAdminListColumnsBuilder(
        IEnumerable<IAdminListColumnsProvider> providers,
        ILogger<DefaultAdminListColumnsBuilder> logger)
    {
        _providers = providers;
        _logger = logger;
    }

    public async Task<AdminListColumnsContext> BuildColumnsAsync(string listName, IEnumerable<AdminListColumn> columns, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new AdminListColumnsContext(listName, columns, data);

        await _providers.InvokeAsync(
            static (provider, context) => provider.PopulateAsync(context),
            context,
            _logger);

        context.Columns.SortByPosition();
        return context;
    }

}
