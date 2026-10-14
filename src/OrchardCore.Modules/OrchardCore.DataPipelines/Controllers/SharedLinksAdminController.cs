using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;
using OrchardCore.DataPipelines.SharedLinks;
using OrchardCore.DataPipelines.ViewModels;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Settings;

namespace OrchardCore.DataPipelines.Controllers;

/// <summary>
/// Lists the shared links of every module, and revokes and deletes them.
/// </summary>
[Admin]
[Feature(DataPipelinesConstants.CoreFeatureId)]
public sealed class SharedLinksAdminController : Controller
{
    private readonly SharedLinkManager _manager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly IShapeFactory _shapeFactory;
    private readonly INotifier _notifier;
    private readonly IClock _clock;

    internal readonly IHtmlLocalizer H;

    public SharedLinksAdminController(
        SharedLinkManager manager,
        IAuthorizationService authorizationService,
        ISiteService siteService,
        IShapeFactory shapeFactory,
        INotifier notifier,
        IClock clock,
        IHtmlLocalizer<SharedLinksAdminController> htmlLocalizer)
    {
        _manager = manager;
        _authorizationService = authorizationService;
        _siteService = siteService;
        _shapeFactory = shapeFactory;
        _notifier = notifier;
        _clock = clock;
        H = htmlLocalizer;
    }

    [Admin("SharedLinks", "SharedLinks")]
    public async Task<IActionResult> Index(string q, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SharedLinkPermissions.ManageSharedLinks))
        {
            return Forbid();
        }

        var pager = new Pager(pagerParameters, (await _siteService.GetSiteSettingsAsync()).PageSize);
        var (links, count) = await _manager.ListAsync(q, pager.GetStartIndex(), pager.PageSize, HttpContext.RequestAborted);

        return View(new SharedLinksViewModel
        {
            Search = q,
            TotalCount = count,
            Links = links.ToList(),
            KindNames = links
                .Select(link => link.Kind)
                .Distinct(StringComparer.Ordinal)
                .ToDictionary(kind => kind, kind => _manager.FindHandler(kind)?.DisplayName.Value ?? kind, StringComparer.Ordinal),
            SourceUrls = links
                .Select(link => (link.LinkId, Url: _manager.FindHandler(link.Kind)?.GetSourceUrl(link, Url)))
                .Where(entry => !string.IsNullOrEmpty(entry.Url))
                .ToDictionary(entry => entry.LinkId, entry => entry.Url, StringComparer.Ordinal),
            UtcNow = _clock.UtcNow,
            Pager = await _shapeFactory.PagerAsync(pager, count, new RouteData(new RouteValueDictionary { ["q"] = q })),
        });
    }

    [HttpPost]
    [Admin("SharedLinks/{linkId}/Revoke", "SharedLinksRevoke")]
    public async Task<IActionResult> Revoke(string linkId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SharedLinkPermissions.ManageSharedLinks))
        {
            return Forbid();
        }

        var link = await _manager.GetAsync(linkId, HttpContext.RequestAborted);

        if (link is null)
        {
            return NotFound();
        }

        await _manager.RevokeAsync(link, HttpContext.RequestAborted);
        await _notifier.SuccessAsync(H["The link to '{0}' no longer works.", link.Title]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Admin("SharedLinks/{linkId}/Delete", "SharedLinksDelete")]
    public async Task<IActionResult> Delete(string linkId)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SharedLinkPermissions.ManageSharedLinks))
        {
            return Forbid();
        }

        var link = await _manager.GetAsync(linkId, HttpContext.RequestAborted);

        if (link is null)
        {
            return NotFound();
        }

        await _manager.DeleteAsync(link, HttpContext.RequestAborted);
        await _notifier.SuccessAsync(H["The link to '{0}' was deleted.", link.Title]);

        return RedirectToAction(nameof(Index));
    }
}
