using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Theming;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListLayoutResolver : IAdminListLayoutResolver
{
    // A shape table is built once per theme and replaced when the tenant reloads, so the layouts it declares are
    // found once, and dropped with it.
    private static readonly ConditionalWeakTable<ShapeTable, ReadOnlyCollection<string>> _layoutsByShapeTable = new();

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

        // The shape table is per theme, and the layouts it declares are found once for it.
        var shapeTable = await _themeManager.GetShapeTableAsync(_shapeTableManager);
        var layouts = _layoutsByShapeTable.GetValue(shapeTable, static table => FindLayouts(table));

        // A custom layout the site renders its lists with gets back the name the settings store, e.g. Cards.
        var defaultLayout = _options.CurrentValue.DefaultLayout;

        for (var i = 0; i < layouts.Count; i++)
        {
            if (!string.Equals(layouts[i], defaultLayout, StringComparison.Ordinal) &&
                string.Equals(layouts[i], defaultLayout, StringComparison.OrdinalIgnoreCase))
            {
                var renamed = layouts.ToArray();
                renamed[i] = defaultLayout;

                return _availableLayouts = Array.AsReadOnly(renamed);
            }
        }

        return _availableLayouts = layouts;
    }

    // A layout is available when it declares itself with an option shape, which is what the admin settings list in
    // their dropdown. The shape table lowercases the shapes of templates, e.g. adminlistlayout_option__grid, so the
    // shipped layouts get back the name the settings store, e.g. Grid.
    private static ReadOnlyCollection<string> FindLayouts(ShapeTable shapeTable)
    {
        string[] shippedLayouts = [AdminListConstants.List, AdminListConstants.Grid];

        return AdminListOptionShapes.GetNames(shapeTable, AdminListConstants.OptionShapePrefix)
            .Select(name => shippedLayouts.FirstOrDefault(shipped => string.Equals(shipped, name, StringComparison.OrdinalIgnoreCase)) ?? name)
            .ToList()
            .AsReadOnly();
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

        var layouts = await GetAvailableLayoutsAsync(cancellationToken);
        var name = layout.AsSpan().Trim();

        for (var i = 0; i < layouts.Count; i++)
        {
            if (name.Equals(layouts[i], StringComparison.OrdinalIgnoreCase))
            {
                return layouts[i];
            }
        }

        return null;
    }
}
