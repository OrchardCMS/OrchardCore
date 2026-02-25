using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.ContentLocalization.Controllers;

[Admin("ContentLocalization/{action}/{id?}", "ContentLocalization{action}")]
public sealed class AdminController : Controller
{
    private readonly IContentDefinitionManager _contentDefinitionManager;
    private readonly IContentManager _contentManager;
    private readonly IContentLocalizationManager _contentLocalizationManager;
    private readonly INotifier _notifier;
    private readonly IAuthorizationService _authorizationService;

    internal readonly IHtmlLocalizer H;

    public AdminController(
        IContentDefinitionManager contentDefinitionManager,
        IContentManager contentManager,
        INotifier notifier,
        IContentLocalizationManager localizationManager,
        IHtmlLocalizer<AdminController> localizer,
        IAuthorizationService authorizationService)
    {
        _contentDefinitionManager = contentDefinitionManager;
        _contentManager = contentManager;
        _notifier = notifier;
        _authorizationService = authorizationService;
        _contentLocalizationManager = localizationManager;
        H = localizer;
    }

    [HttpPost]
    public async Task<IActionResult> Localize(string contentItemId, string targetCulture, string returnUrl = null)
    {
        // Invariant culture name is empty so a null value is bound.
        targetCulture ??= string.Empty;

        var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, ContentLocalizationPermissions.LocalizeContent, contentItem))
        {
            return Forbid();
        }

        if (!await _authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.EditContent, contentItem.ContentType, User.Identity.Name))
        {
            return Forbid();
        }

        var part = contentItem.As<LocalizationPart>();

        if (part == null)
        {
            return NotFound();
        }

        var alreadyLocalizedContent = await _contentLocalizationManager.GetContentItemAsync(part.LocalizationSet, targetCulture);

        if (alreadyLocalizedContent != null)
        {
            await _notifier.WarningAsync(H["A localization already exists for '{0}'.", targetCulture]);
            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId, returnUrl });
        }

        try
        {
            var newContent = await _contentLocalizationManager.LocalizeAsync(contentItem, targetCulture);
            await _notifier.InformationAsync(H["Localized version of the content created successfully."]);
            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = newContent.ContentItemId, returnUrl });
        }
        catch (InvalidOperationException)
        {
            await _notifier.WarningAsync(H["Could not create localized version of the content item."]);
            return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = contentItem.ContentItemId, returnUrl });
        }
    }

    public async Task<IActionResult> Settings()
    {
        if (!await _authorizationService.AuthorizeAsync(User, ContentLocalizationPermissions.ManageContentLocalizationSettings))
        {
            return Unauthorized();
        }

        var contentTypes = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        var contentTypeEntries = contentTypes
            .Select(type => new ContentTypeEntry
            {
                Name = type.Name,
                DisplayName = type.DisplayName,
                IsLocalized = type.Parts.Any(part => part.Name == nameof(LocalizationPart)),
            })
            .OrderBy(type => type.DisplayName)
            .ToList();

        return View(new LocalizationSettingsViewModel
        {
            ContentTypeEntries = contentTypeEntries,
            SelectedContentTypes = [],
        });
    }

    [HttpPost]
    [ActionName(nameof(Settings))]
    public async Task<IActionResult> SettingsPost(LocalizationSettingsViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, ContentLocalizationPermissions.ManageContentLocalizationSettings))
        {
            return Unauthorized();
        }

        var selected = new HashSet<string>(model.SelectedContentTypes ?? [], StringComparer.OrdinalIgnoreCase);

        var contentTypes = await _contentDefinitionManager.ListTypeDefinitionsAsync();

        foreach (var contentType in contentTypes)
        {
            var hasLocalizationPart = contentType.Parts.Any(part => part.Name == nameof(LocalizationPart));
            var shouldHaveLocalizationPart = selected.Contains(contentType.Name);

            if (hasLocalizationPart == shouldHaveLocalizationPart)
            {
                continue;
            }

            await _contentDefinitionManager.AlterTypeDefinitionAsync(contentType.Name, type =>
            {
                if (shouldHaveLocalizationPart)
                {
                    type.WithPart(nameof(LocalizationPart));
                }
                else
                {
                    type.RemovePart(nameof(LocalizationPart));
                }
            });
        }

        await _notifier.SuccessAsync(H["Content localization settings saved successfully."]);

        return RedirectToAction(nameof(Settings));
    }
}
