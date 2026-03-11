using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Contents.VersionPruning.Drivers;
using OrchardCore.Contents.VersionPruning.Models;
using OrchardCore.Contents.VersionPruning.Services;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;

namespace OrchardCore.Contents.VersionPruning.Controllers;

public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentVersionPruningService _pruningService;
    private readonly ISiteService _siteService;
    private readonly IClock _clock;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        IContentVersionPruningService pruningService,
        ISiteService siteService,
        IClock clock,
        INotifier notifier,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _pruningService = pruningService;
        _siteService = siteService;
        _clock = clock;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    [HttpPost]
    public async Task<IActionResult> Prune()
    {
        if (!await _authorizationService.AuthorizeAsync(User, ContentVersionPruningPermissions.ManageContentVersionPruningSettings))
        {
            return Forbid();
        }

        var settings = await _siteService.GetSettingsAsync<ContentVersionPruningSettings>();

        var pruned = await _pruningService.PruneVersionsAsync(settings);

        var container = await _siteService.LoadSiteSettingsAsync();
        container.Alter<ContentVersionPruningSettings>(nameof(ContentVersionPruningSettings), settings =>
        {
            settings.LastRunUtc = _clock.UtcNow;
        });

        await _siteService.UpdateSiteSettingsAsync(container);

        await _notifier.SuccessAsync(H["Content version pruning completed. {0} version(s) deleted.", pruned]);

        return RedirectToAction("Index", "Admin", new
        {
            area = "OrchardCore.Settings",
            groupId = ContentVersionPruningSettingsDisplayDriver.GroupId,
        });
    }
}
