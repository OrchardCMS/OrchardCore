using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin.Models;
using OrchardCore.Modules;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListColumnsBuilder : IAdminListColumnsBuilder
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public DefaultAdminListColumnsBuilder(
        IServiceProvider serviceProvider,
        ILogger<DefaultAdminListColumnsBuilder> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<AdminListColumnsContext> BuildColumnsAsync(string listName, IEnumerable<AdminListColumn> columns, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var context = new AdminListColumnsContext(listName, columns, data);

        var providers = _serviceProvider.GetServices<IAdminListColumnsProvider>()
            .Concat(_serviceProvider.GetKeyedServices<IAdminListColumnsProvider>(new AdminListColumnsProviderKey(listName)));

        await providers.InvokeAsync(
            static (provider, context) => provider.PopulateAsync(context),
            context,
            _logger);

        context.Columns.SortByPosition();
        return context;
    }

}
