using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Admin.Services;

internal sealed class AdminListLayoutResolver
{
    private readonly IOptionsMonitor<AdminListOptions> _options;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AdminListLayoutPreference _layoutPreference;

    public AdminListLayoutResolver(
        IOptionsMonitor<AdminListOptions> options,
        IHttpContextAccessor httpContextAccessor,
        AdminListLayoutPreference layoutPreference)
    {
        _options = options;
        _httpContextAccessor = httpContextAccessor;
        _layoutPreference = layoutPreference;
    }

    public bool AllowUserSelection
        => _options.CurrentValue.AllowUserSelection && _httpContextAccessor.HttpContext != null;

    public AdminListLayout GetLayout(string listName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(listName);

        var options = _options.CurrentValue;
        if (!options.AllowUserSelection)
        {
            return options.DefaultLayout;
        }

        var query = _httpContextAccessor.HttpContext?.Request.Query[AdminListLayoutPreference.QueryKey].ToString();
        if (TryParse(query, out var queried))
        {
            _layoutPreference.Set(listName, queried.ToString());
            return queried;
        }

        _layoutPreference.TryGet(listName, out var preferred);
        return TryParse(preferred, out var layout) ? layout : options.DefaultLayout;
    }

    public string GetLayoutUrl(AdminListLayout layout)
    {
        if (!Enum.IsDefined(layout))
        {
            throw new ArgumentOutOfRangeException(nameof(layout), layout, "An admin list layout must be List or Grid.");
        }

        var request = _httpContextAccessor.HttpContext.Request;
        var query = request.Query
            .Where(pair => !string.Equals(pair.Key, AdminListLayoutPreference.QueryKey, StringComparison.OrdinalIgnoreCase))
            .Append(new KeyValuePair<string, StringValues>(AdminListLayoutPreference.QueryKey, layout.ToString()));

        return request.PathBase + request.Path + QueryString.Create(query);
    }

    private static bool TryParse(string value, out AdminListLayout layout)
    {
        var name = value.AsSpan().Trim();
        if (name.Equals(nameof(AdminListLayout.List), StringComparison.OrdinalIgnoreCase))
        {
            layout = AdminListLayout.List;
            return true;
        }

        if (name.Equals(nameof(AdminListLayout.Grid), StringComparison.OrdinalIgnoreCase))
        {
            layout = AdminListLayout.Grid;
            return true;
        }

        layout = default;
        return false;
    }
}
