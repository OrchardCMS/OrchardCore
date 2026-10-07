using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin.QuickNavigation;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Settings;
using YesSql;
using ISession = YesSql.ISession;

namespace OrchardCore.Contents.QuickNavigation;

public sealed class ContentItemNavigationSource : QuickNavigationSource
{
    private readonly ISession _session;
    private readonly IContentManager _contentManager;
    private readonly ISiteService _siteService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LinkGenerator _linkGenerator;
    private readonly Dictionary<string, ContentItem> _items = new(StringComparer.Ordinal);

    internal readonly IStringLocalizer S;

    public ContentItemNavigationSource(
        ISession session,
        IContentManager contentManager,
        ISiteService siteService,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        LinkGenerator linkGenerator,
        IStringLocalizer<ContentItemNavigationSource> localizer)
    {
        _session = session;
        _contentManager = contentManager;
        _siteService = siteService;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _linkGenerator = linkGenerator;
        S = localizer;
    }

    public override string Name => nameof(ContentItemNavigationSource);

    public override async ValueTask<IEnumerable<string>> GetEntryIdsAsync()
    {
        var settings = await _siteService.GetSettingsAsync<ContentQuickNavigationSettings>();
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(settings.MaxItems);

        var items = await _session.Query<ContentItem, ContentItemIndex>(item => item.Latest)
            .OrderByDescending(item => item.ModifiedUtc)
            .ThenBy(item => item.ContentItemId)
            .Take(settings.MaxItems)
            .ListAsync();

        _items.Clear();
        foreach (var item in items)
        {
            _items.Add(item.ContentItemId, item);
        }

        return _items.Keys;
    }

    public override async ValueTask<QuickNavigationResult> DisplayAsync(string entryId)
    {
        if (!_items.TryGetValue(entryId, out var item))
        {
            return null;
        }

        item = await _contentManager.LoadAsync(item);
        var httpContext = _httpContextAccessor.HttpContext;
        if (string.IsNullOrWhiteSpace(item.DisplayText) ||
            !await _authorizationService.AuthorizeAsync(httpContext?.User, CommonPermissions.EditContent, item))
        {
            return null;
        }

        var url = _linkGenerator.GetPathByRouteValues(httpContext, "EditContentItem", new { contentItemId = item.ContentItemId });
        if (string.IsNullOrEmpty(url))
        {
            throw new InvalidOperationException("The EditContentItem route is not available.");
        }

        return new QuickNavigationResult
        {
            Title = item.DisplayText,
            Path = [S["Content"]],
            Href = url,
        };
    }
}
