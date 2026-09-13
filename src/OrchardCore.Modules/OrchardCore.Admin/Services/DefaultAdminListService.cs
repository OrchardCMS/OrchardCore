using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;
using OrchardCore.DisplayManagement.Zones;
using OrchardCore.Modules;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListService : IAdminListService
{
    private readonly IEnumerable<IAdminListColumnProvider> _columnProviders;
    private readonly IOptionsMonitor<AdminListOptions> _options;
    private readonly ILogger _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AdminListLayoutPreference _layoutPreference;
    private readonly IThemeManager _themeManager;
    private readonly IShapeTableManager _shapeTableManager;

    private IList<string> _availableLayouts;

    public DefaultAdminListService(
        IEnumerable<IAdminListColumnProvider> columnProviders,
        IOptionsMonitor<AdminListOptions> options,
        ILogger<DefaultAdminListService> logger,
        IHttpContextAccessor httpContextAccessor,
        AdminListLayoutPreference layoutPreference,
        IThemeManager themeManager,
        IShapeTableManager shapeTableManager)
    {
        _columnProviders = columnProviders;
        _options = options;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _layoutPreference = layoutPreference;
        _themeManager = themeManager;
        _shapeTableManager = shapeTableManager;
    }

    // The options carry the effective defaults: the tenant configuration, overridden by the site settings.
    // They are guarded so a blank value cannot leave a list without a layout.
    private string DefaultLayout
        => string.IsNullOrWhiteSpace(_options.CurrentValue.DefaultLayout)
            ? AdminListConstants.List
            : _options.CurrentValue.DefaultLayout.Trim();

    private string DefaultActionsLayout
        => string.IsNullOrWhiteSpace(_options.CurrentValue.DefaultActionsLayout)
            ? AdminListActionsLayouts.Buttons
            : _options.CurrentValue.DefaultActionsLayout.Trim();

    public async Task<string> GetLayoutAsync(string listName, string requestedLayout = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        cancellationToken.ThrowIfCancellationRequested();

        // What the page asked for wins, whatever the site allows: it is the page rendering itself.
        if (!string.IsNullOrWhiteSpace(requestedLayout))
        {
            return requestedLayout.Trim();
        }

        if (!_options.CurrentValue.AllowUserSelection)
        {
            return DefaultLayout;
        }

        // The selector of a list links to the same page with the layout it offers, e.g. ?layout=Grid.
        var queried = _httpContextAccessor.HttpContext?.Request.Query[AdminListLayoutPreference.QueryKey].ToString();

        if (!string.IsNullOrWhiteSpace(queried) && await IsAvailableAsync(queried, cancellationToken))
        {
            queried = queried.Trim();

            // Remembered, so the next page of the list, and the next visit, open the same way.
            _layoutPreference.Set(listName, queried);

            return queried;
        }

        if (_layoutPreference.TryGet(listName, out var preferred) && await IsAvailableAsync(preferred, cancellationToken))
        {
            return preferred.Trim();
        }

        return DefaultLayout;
    }

    public async Task<IList<string>> GetAvailableLayoutsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_availableLayouts != null)
        {
            return _availableLayouts;
        }

        // A layout is available when it declares itself with an option shape, which is what the admin settings
        // list in its dropdown. The shape table is per theme, so this is resolved per request.
        var theme = await _themeManager.GetThemeAsync();
        var shapeTable = await _shapeTableManager.GetShapeTableAsync(theme?.Id);

        // A binding is keyed by a lowercase name but remembers the one it was declared with, e.g. Grid, which
        // is what a link and a template name should carry.
        _availableLayouts = shapeTable.Bindings
            .Where(binding => binding.Key.StartsWith(AdminListConstants.OptionShapePrefix, StringComparison.OrdinalIgnoreCase))
            .Select(binding => (binding.Value?.BindingName ?? binding.Key)[AdminListConstants.OptionShapePrefix.Length..])
            .Where(layout => !string.IsNullOrEmpty(layout))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(layout => layout, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return _availableLayouts;
    }

    public async Task<IList<AdminListLayoutOption>> GetLayoutOptionsAsync(string listName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null || !_options.CurrentValue.AllowUserSelection)
        {
            return [];
        }

        var layouts = await GetAvailableLayoutsAsync(cancellationToken);

        // Nothing to offer when the site renders its lists one way.
        if (layouts.Count < 2)
        {
            return [];
        }

        // A page can render the same list several times, e.g. the features and the recipes, which render one
        // list per group, and a page can place the selector itself instead of leaving it to its lists. They
        // all share one layout, so the first caller of the request is the one offering it.
        if (!httpContext.Items.TryAdd($"{AdminListConstants.LayoutSelectorShapeType}:{listName}", true))
        {
            return [];
        }

        return layouts
            .Select(layout => new AdminListLayoutOption
            {
                Name = layout,
                Url = BuildLayoutUrl(httpContext.Request, layout),
            })
            .ToList();
    }

    // The same page, rendered with another layout. The rest of the query string is kept, so switching layout
    // holds on to the search, the filters and the page the user is on.
    private static string BuildLayoutUrl(HttpRequest request, string layout)
    {
        var parameters = new List<KeyValuePair<string, string>>();

        foreach (var pair in QueryHelpers.ParseQuery(request.QueryString.Value))
        {
            if (string.Equals(pair.Key, AdminListLayoutPreference.QueryKey, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            foreach (var value in pair.Value)
            {
                parameters.Add(new KeyValuePair<string, string>(pair.Key, value));
            }
        }

        parameters.Add(new KeyValuePair<string, string>(AdminListLayoutPreference.QueryKey, layout));

        return QueryHelpers.AddQueryString((request.PathBase + request.Path).Value, parameters);
    }

    private async Task<bool> IsAvailableAsync(string layout, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(layout))
        {
            return false;
        }

        var layouts = await GetAvailableLayoutsAsync(cancellationToken);

        return layouts.Contains(layout.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    public Task<string> GetActionsLayoutAsync(string listName = null, string requestedLayout = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(string.IsNullOrWhiteSpace(requestedLayout)
            ? DefaultActionsLayout
            : requestedLayout.Trim());
    }

    public async Task<IList<AdminListColumn>> GetColumnsAsync(string listName, IEnumerable<AdminListColumn> defaultColumns, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        var columns = defaultColumns?.ToList() ?? [];

        // The default columns get increasing positions (10, 20, ...) when the list owner did not assign any,
        // so providers can insert a column between two of them (e.g. "15") regardless of the order they run in.
        for (var i = 0; i < columns.Count; i++)
        {
            if (string.IsNullOrWhiteSpace(columns[i].Position))
            {
                columns[i].Position = ((i + 1) * 10).ToString(CultureInfo.InvariantCulture);
            }
        }

        var context = new AdminListColumnsContext(listName, columns, data);

        await _columnProviders.InvokeAsync(
            static (provider, context, cancellationToken) => provider.BuildAsync(context, cancellationToken),
            context,
            cancellationToken,
            _logger);

        // Columns added without a position go after all the positioned ones. The sort is stable, so columns
        // sharing a position keep the order they were added in.
        return context.Columns
            .OrderBy(column => string.IsNullOrWhiteSpace(column.Position) ? "after" : column.Position, FlatPositionComparer.Instance)
            .ToList();
    }
}
