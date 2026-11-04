using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Media.Services;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Media.ViewModels;
using OrchardCore.Modules;

namespace OrchardCore.Media.Controllers;

[Feature("OrchardCore.Media.Cache")]
[Admin("MediaCache/{action}", "MediaCache.{action}")]
public sealed class MediaCacheController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly MediaCacheManagementService _cache;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public MediaCacheController(
        IAuthorizationService authorizationService,
        IServiceProvider serviceProvider,
        INotifier notifier,
        IHtmlLocalizer<MediaCacheController> htmlLocalizer
        )
    {
        _authorizationService = authorizationService;
        // Resolve from service provider as the service will not be registered if configuration is invalid.
        _cache = new MediaCacheManagementService(serviceProvider);
        _notifier = notifier;
        H = htmlLocalizer;
    }

    [Admin("MediaCache", "MediaCache.Index")]
    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageAssetCache))
        {
            return Forbid();
        }
        var model = new MediaCacheViewModel
        {
            IsConfigured = _cache.RemoteConfigured,
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Purge()
    {
        if (!await _authorizationService.AuthorizeAsync(User, MediaPermissions.ManageAssetCache))
        {
            return Forbid();
        }

        if (!_cache.RemoteConfigured)
        {
            await _notifier.ErrorAsync(H["The asset cache feature is enabled, but a remote media store feature is not enabled, or not configured with appsettings.json."]);
            return RedirectToAction(nameof(Index));
        }

        var result = await _cache.PurgeAsync("remote", HttpContext.RequestAborted);
        if (result != MediaCachePurgeStatus.Purged)
        {
            await _notifier.ErrorAsync(H["Asset cache purged, with errors."]);
        }
        else
        {
            await _notifier.InformationAsync(H["Asset cache purged."]);
        }

        return RedirectToAction(nameof(Index));
    }
}
