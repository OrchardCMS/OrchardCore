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
                .Where(index => index.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync())
                ?.As<AuditTrailContentEvent>()
                ?.ContentItem;

            if (contentItem == null)
            {
                return NotFound();
            }

            contentItem.Id = 0;
            contentItem.ContentItemVersionId = "";
            contentItem.Published = false;
            contentItem.Latest = false;

            contentItem = await _contentManager.LoadAsync(contentItem);

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
                .Where(index => index.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync())
                ?.As<AuditTrailContentEvent>()
                ?.ContentItem;

            if (contentItem == null)
            {
                return NotFound();
            }

            // So that a new record will be created.
            contentItem.Id = 0;

            // So that a new version id will be generated.
            contentItem.ContentItemVersionId = "";

            contentItem.Latest = contentItem.Published = false;

            contentItem = await _contentManager.LoadAsync(contentItem);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, contentItem))
            {
                return Forbid();
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

            var latestVersion = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(index => index.ContentItemId == contentItem.ContentItemId && index.Latest)
                .FirstOrDefaultAsync();

            if (contentItem.ContentItemVersionId == latestVersion.ContentItemVersionId)
            {
                _notifier.Warning(H["'{0}' was not restored, the version is already active.", contentItem.DisplayText]);
                return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
            }

            var context = new RestoreContentContext(contentItem);

            await _auditTrailContentHandlers.InvokeAsync((handler, context) => handler.RestoringAsync(context), context, _logger);

            // Remove an existing draft but keep an existing published version.
            if (latestVersion != null)
            {
                latestVersion.Latest = false;
                _session.Save(latestVersion);
            }

            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            await _auditTrailContentHandlers.InvokeAsync((handler, context) => handler.RestoredAsync(context), context, _logger);

            _notifier.Success(H["'{0}' has been restored.", contentItem.DisplayText]);
            return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
        }
    }
}
