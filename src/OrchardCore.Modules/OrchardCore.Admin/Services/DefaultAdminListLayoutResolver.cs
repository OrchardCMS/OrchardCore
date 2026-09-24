using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListLayoutResolver : IAdminListLayoutResolver
{
    private readonly IOptionsMonitor<AdminListOptions> _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AdminListLayoutPreference _layoutPreference;
    private readonly IThemeManager _themeManager;
    private readonly IShapeTableManager _shapeTableManager;

    private IList<string> _availableLayouts;

    public DefaultAdminListLayoutResolver(
        IOptionsMonitor<AdminListOptions> options,
        IHttpContextAccessor httpContextAccessor,
        AdminListLayoutPreference layoutPreference,
        IThemeManager themeManager,
        IShapeTableManager shapeTableManager)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
        _layoutPreference = layoutPreference;
        _themeManager = themeManager;
        _shapeTableManager = shapeTableManager;
    }

    public async Task<string> GetLayoutAsync(string listName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        // The options carry the effective default: the tenant configuration, overridden by the site settings.
        var options = _options.CurrentValue;

        if (!options.AllowUserSelection)
        {
            return options.DefaultLayout;
        }

        // The selector of a list links to the same page with the layout it offers, e.g. ?layout=Grid.
        var queried = await FindAvailableLayoutAsync(_httpContextAccessor.HttpContext?.Request.Query[AdminListLayoutPreference.QueryKey].ToString(), cancellationToken);

        if (queried != null)
        {
            // Remembered, so the next page of the list, and the next visit, open the same way.
            _layoutPreference.Set(listName, queried);

            return queried;
        }

        _layoutPreference.TryGet(listName, out var preferred);

        return await FindAvailableLayoutAsync(preferred, cancellationToken) ?? options.DefaultLayout;
    }

    public async Task<IList<string>> GetAvailableLayoutsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_availableLayouts != null)
        {
            return _availableLayouts;
        }

        // A layout is available when it declares itself with an option shape, which is what the admin settings
        // list in their dropdown. The shape table is per theme, so this is resolved per request.
        var shapeTable = await _themeManager.GetShapeTableAsync(_shapeTableManager);

        // The shape table lowercases the shapes of templates, e.g. adminlistlayout_option__grid, so the shipped
        // layouts and the default of the site get back the name the settings store, e.g. Grid.
        string[] knownLayouts = [_options.CurrentValue.DefaultLayout, AdminListConstants.List, AdminListConstants.Table, AdminListConstants.Grid];

        return _availableLayouts = AdminListOptionShapes.GetNames(shapeTable, AdminListConstants.OptionShapePrefix)
            .Select(name => knownLayouts.FirstOrDefault(known => string.Equals(known, name, StringComparison.OrdinalIgnoreCase)) ?? name)
            .ToList();
    }

    public async Task<IList<AdminListLayoutOption>> GetLayoutOptionsAsync(CancellationToken cancellationToken = default)
    {
        var request = _httpContextAccessor.HttpContext?.Request;

        if (request is null || !_options.CurrentValue.AllowUserSelection)
        {
            return [];
        }

        var layouts = await GetAvailableLayoutsAsync(cancellationToken);

        // Nothing to offer when the site renders its lists one way.
        if (layouts.Count < 2)
        {
            return [];
        }

        // The same page, rendered with another layout. The rest of the query string is kept, so switching layout
        // holds on to the search, the filters and the page the user is on.
        var path = request.PathBase + request.Path;
        var query = request.Query
            .Where(pair => !string.Equals(pair.Key, AdminListLayoutPreference.QueryKey, StringComparison.OrdinalIgnoreCase))
            .ToList();

        return layouts
            .Select(layout => new AdminListLayoutOption
            {
                Name = layout,
                Url = path + QueryString.Create(query.Append(new KeyValuePair<string, StringValues>(AdminListLayoutPreference.QueryKey, layout))),
            })
            .ToList();
    }

    // The layout as this site names it, e.g. Grid for ?layout=grid, or null when this site cannot render it: a
    // layout name would otherwise reach the AdminList__{Name} alternates of another list.
    private async Task<string> FindAvailableLayoutAsync(string layout, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(layout))
        {
            return null;
        }

        layout = layout.Trim();

        var layouts = await GetAvailableLayoutsAsync(cancellationToken);

        return layouts.FirstOrDefault(available => string.Equals(available, layout, StringComparison.OrdinalIgnoreCase));
    }
}
