using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.AuditTrail.Services;
using OrchardCore.AuditTrail.Services.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Contents.AuditTrail.Controllers
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    [Admin]
    public class ContentController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IEnumerable<IAuditTrailContentHandler> _auditTrailContentHandlers;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;
        private readonly ILogger _logger;

        public ContentController(
            ISession session,
            IContentManager contentManager,
            IUpdateModelAccessor updateModelAccessor,
            IAuthorizationService authorizationService,
            IContentItemDisplayManager contentItemDisplayManager,
            IEnumerable<IAuditTrailContentHandler> auditTrailContentHandlers,
            INotifier notifier,
            IHtmlLocalizer<ContentController> htmlLocalizer,
            ILogger<ContentController> logger)
        {
            _session = session;
            _contentManager = contentManager;
            _updateModelAccessor = updateModelAccessor;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _auditTrailContentHandlers = auditTrailContentHandlers;
            _notifier = notifier;
            H = htmlLocalizer;
            _logger = logger;
        }

        public async Task<ActionResult> Detail(int versionNumber, string auditTrailEventId)
        {
            var contentItem = (await _session.Query<AuditTrailEvent, AuditTrailEventIndex>()
                .Where(auditTrailEventIndex => auditTrailEventIndex.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync())
                ?.As<AuditTrailContentEvent>()
                ?.ContentItem;

            if (String.IsNullOrEmpty(contentItem?.ContentItemVersionId))
            {
                return NotFound();
            }

            if (!contentItem.CreatedUtc.HasValue)
            {
                contentItem = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);
                if (contentItem == null)
                {
                    return NotFound();
                }
            }
            else
            {
                contentItem = await _contentManager.LoadAsync(contentItem);
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            var auditTrailPart = contentItem.As<AuditTrailPart>();
            if (auditTrailPart != null)
            {
                auditTrailPart.ShowComment = true;
            }

            var model = await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

            model.Properties["VersionNumber"] = versionNumber;

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Restore(string auditTrailEventId)
        {
            var contentItem = (await _session.Query<AuditTrailEvent, AuditTrailEventIndex>()
                .Where(auditTrailEventIndex => auditTrailEventIndex.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync())
                ?.As<AuditTrailContentEvent>()
                ?.ContentItem;

            if (String.IsNullOrEmpty(contentItem?.ContentItemVersionId))
            {
                return NotFound();
            }

            if (!contentItem.CreatedUtc.HasValue)
            {
                contentItem = await _contentManager.GetVersionAsync(contentItem.ContentItemVersionId);
                if (contentItem == null)
                {
                    return NotFound();
                }
            }
            else
            {
                contentItem = await _contentManager.LoadAsync(contentItem);
                contentItem.Latest = contentItem.Published = false;
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, contentItem))
            {
                return Forbid();
            }

            if (contentItem.Latest || contentItem.Published)
            {
                _notifier.Warning(H["'{0}' was not restored, the version is already active.", contentItem.DisplayText]);
                return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
            }

            var result = await _contentManager.ValidateAsync(contentItem);
            if (!result.Succeeded)
            {
                _notifier.Warning(H["'{0}' was not restored, the version is not valid.", contentItem.DisplayText]);
                foreach (var error in result.Errors)
                {
                    _notifier.Warning(H[error.ErrorMessage]);
                }

                return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
            }

            // So that a new record will be created.
            contentItem.Id = 0;

            //await _auditTrailContentHandlers.RestoringAsync(new RestoreContentContext(contentItem));

            var context = new RestoreContentContext(contentItem);

            await _auditTrailContentHandlers.InvokeAsync((handler, context) => handler.RestoringAsync(context), context, _logger);

            // Remove an existing draft but keep an existing published version.
            var latestVersion = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(i => i.ContentItemId == contentItem.ContentItemId && i.Latest)
                .FirstOrDefaultAsync();

            if (latestVersion != null)
            {
                latestVersion.Latest = false;
                _session.Save(latestVersion);
            }

            // So that a new version will be generated.
            contentItem.ContentItemVersionId = String.Empty;

            // Create a new draft from the version to restore.
            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            await _auditTrailContentHandlers.InvokeAsync((handler, context) => handler.RestoredAsync(context), context, _logger);

            _notifier.Success(H["'{0}' has been restored.", contentItem.DisplayText]);
            return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
        }
    }
}
