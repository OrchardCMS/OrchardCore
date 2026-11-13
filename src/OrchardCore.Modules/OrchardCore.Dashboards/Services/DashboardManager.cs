using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Dashboards.Indexes;
using OrchardCore.Dashboards.Models;
using YesSql;

namespace OrchardCore.Dashboards.Services;

public sealed class DashboardManager : IDashboardManager
{
    private readonly ISession _session;
    private readonly IAuthorizationService _authorizationService;
    private readonly DashboardOptions _options;

    public DashboardManager(
        ISession session,
        IAuthorizationService authorizationService,
        IOptions<DashboardOptions> options)
    {
        _session = session;
        _authorizationService = authorizationService;
        _options = options.Value;
    }

    public DashboardTypeDescriptor GetType(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return _options.Types.TryGetValue(name, out var type) ? type : null;
    }

    public IEnumerable<DashboardTypeDescriptor> GetTypes()
        => _options.Types.Values;

    public async Task<bool> CanViewAsync(ClaimsPrincipal user, DashboardTypeDescriptor type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (await CanManageAsync(user, type))
        {
            return true;
        }

        if (type.Scope == DashboardScope.User || type.ViewPermission is null || user is null)
        {
            return false;
        }

        return await _authorizationService.AuthorizeAsync(user, type.ViewPermission);
    }

    public async Task<bool> CanManageAsync(ClaimsPrincipal user, DashboardTypeDescriptor type)
    {
        ArgumentNullException.ThrowIfNull(type);

        if (user is null || type.ManagePermission is null)
        {
            return false;
        }

        if (type.Scope == DashboardScope.User && string.IsNullOrEmpty(GetUserId(user)))
        {
            return false;
        }

        return await _authorizationService.AuthorizeAsync(user, type.ManagePermission);
    }

    public async Task<IEnumerable<ContentItem>> GetWidgetsAsync(ClaimsPrincipal user, DashboardTypeDescriptor type, bool latest)
    {
        ArgumentNullException.ThrowIfNull(type);

        var typeName = type.Name;
        IQuery<ContentItem, DashboardWidgetIndex> query;

        if (type.Scope == DashboardScope.User)
        {
            var userId = GetUserId(user);

            if (string.IsNullOrEmpty(userId))
            {
                return [];
            }

            query = _session.Query<ContentItem, DashboardWidgetIndex>(index => index.DashboardType == typeName && index.OwnerId == userId);
        }
        else
        {
            query = _session.Query<ContentItem, DashboardWidgetIndex>(index => index.DashboardType == typeName);
        }

        query = query.OrderBy(index => index.Position);

        return latest
            ? await query.With<ContentItemIndex>(index => index.Latest).ListAsync()
            : await query.With<ContentItemIndex>(index => index.Published).ListAsync();
    }

    public bool IsOnDashboard(ClaimsPrincipal user, DashboardTypeDescriptor type, ContentItem widget)
    {
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(widget);

        if (!widget.TryGet<DashboardPart>(out var part) ||
            !string.Equals(part.DashboardType, type.Name, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (type.Scope == DashboardScope.User)
        {
            var userId = GetUserId(user);

            return !string.IsNullOrEmpty(userId) && string.Equals(part.OwnerId, userId, StringComparison.Ordinal);
        }

        return true;
    }

    internal static string GetUserId(ClaimsPrincipal user)
        => user?.FindFirstValue(ClaimTypes.NameIdentifier);
}
