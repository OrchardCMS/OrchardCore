using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.ContentLocalization.Controllers;

public sealed class AdminController : Controller
{
    private readonly IContentLocalizationService _contentLocalizationService;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        INotifier notifier,
        IContentLocalizationService localizationService,
        IHtmlLocalizer<AdminController> localizer)
    {
        _notifier = notifier;
        _contentLocalizationService = localizationService;
        H = localizer;
    }

    [HttpPost]
    [Admin("ContentLocalization", "ContentLocalization.Localize")]
    public async Task<IActionResult> Localize(string contentItemId, string targetCulture, string returnUrl = null)
    {
        // Invariant culture name is empty so a null value is bound.
        targetCulture ??= string.Empty;

        try
        {
            var result = await _contentLocalizationService.LocalizeAsync(User, contentItemId, targetCulture);
            if (result.Status == ContentLocalizationStatus.NotFound)
            {
                return NotFound();
            }
            if (result.Status == ContentLocalizationStatus.Forbidden)
            {
                return Forbid();
            }
            if (result.Status is ContentLocalizationStatus.Invalid or ContentLocalizationStatus.Conflict)
            {
                await _notifier.WarningAsync(H["Could not create localized version of the content item."]);
            }
            else if (!result.Created)
            {
                await _notifier.WarningAsync(H["A localization already exists for '{0}'.", targetCulture]);
            }
            else
            {
                await _notifier.InformationAsync(H["Localized version of the content created successfully."]);
                contentItemId = result.ContentItem.ContentItemId;
            }
        }
        catch (InvalidOperationException)
        {
            await _notifier.WarningAsync(H["Could not create localized version of the content item."]);
        }
        return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId, returnUrl });
    }
}
