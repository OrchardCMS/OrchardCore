using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;

namespace OrchardCore.ContentLocalization.Controllers
{
    public class AdminController : Controller, IUpdateModel
    {
        private readonly IContentManager _contentManager;
        private readonly IContentLocalizationManager _contentLocalizationManager;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<AdminController> _localizer;

        public IHtmlLocalizer T { get; }

        public AdminController(IContentManager contentManager, INotifier notifier, IContentLocalizationManager localizationManager, IHtmlLocalizer<AdminController> localizer)
        {
            _contentManager = contentManager;
            _notifier = notifier;
            _localizer = localizer;
            _contentLocalizationManager = localizationManager;
            T = localizer;
        }

        public async Task<IActionResult> Localize(string contentItemId, string targetCulture)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }
            var part = contentItem.As<LocalizationPart>();
            if (part == null)
            {
                return NotFound();
            }
            var alreadyLocalizedContent = await _contentLocalizationManager.GetContentItem(part.LocalizationSet, targetCulture);

            if (alreadyLocalizedContent != null)
            {
                _notifier.Warning(T["A Localization already exists for #{0} ", targetCulture]);
                return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = contentItemId });

            }

            // todo: verify if this is required
            //if (!await _authorizationService.AuthorizeAsync(User, Permissions.CreateContent, contentItem))
            //{
            //    return Unauthorized();
            //}

            try
            {
                var newContent = await _contentLocalizationManager.LocalizeAsync(contentItem, targetCulture);
                _notifier.Information(T["Successfully created localized version of the content."]);
                return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = newContent.ContentItemId });
            }
            catch (InvalidOperationException)
            {
                _notifier.Warning(T["Could not create localized version the content item"]);
                return RedirectToAction("Edit", "Admin", new { area = "OrchardCore.Contents", contentItemId = contentItem.ContentItemId });
            }
        }
    }
}