using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Newtonsoft.Json.Linq;
using OrchardCore.Admin;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using YesSql;

namespace OrchardCore.Contents.AuditTrail.Controllers
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    [Admin]
    public class ContentController : Controller
    {
        private static readonly JsonMergeSettings UpdateJsonMergeSettings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Replace };

        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuditTrailContentHandler _auditTrailContentHandler;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer H;

        public ContentController(
            ISession session,
            IContentManager contentManager,
            IUpdateModelAccessor updateModelAccessor,
            IAuthorizationService authorizationService,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuditTrailContentHandler auditTrailContentHandler,
            INotifier notifier,
            IHtmlLocalizer<ContentController> htmlLocalizer)
        {
            _session = session;
            _contentManager = contentManager;
            _updateModelAccessor = updateModelAccessor;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _auditTrailContentHandler = auditTrailContentHandler;
            _notifier = notifier;
            H = htmlLocalizer;
        }

        public async Task<ActionResult> Detail(int versionNumber, string auditTrailEventId)
        {
            var auditTrailEvent = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>()
                .Where(auditTrailEventIndex => auditTrailEventIndex.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync();

            var contentItemToEdit = auditTrailEvent?.Get(auditTrailEvent.EventName)?.ToObject<ContentItem>();
            if (String.IsNullOrEmpty(contentItemToEdit?.ContentItemId) || String.IsNullOrEmpty(contentItemToEdit.ContentType))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.NewAsync(contentItemToEdit.ContentType);
            contentItem.Owner = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            contentItem.Merge(contentItemToEdit);
            contentItem.ContentItemId = contentItemToEdit.ContentItemId;
            contentItem.ContentItemVersionId = String.Empty;

            contentItem = await _contentManager.LoadAsync(contentItem);

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
            var auditTrailEvent = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>()
                .Where(auditTrailEventIndex => auditTrailEventIndex.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync();

            var contentItemToRestore = auditTrailEvent?.Get(auditTrailEvent.EventName)?.ToObject<ContentItem>();
            if (String.IsNullOrEmpty(contentItemToRestore?.ContentItemVersionId))
            {
                return NotFound();
            }

            var contentItem = await _contentManager.GetVersionAsync(contentItemToRestore.ContentItemVersionId);
            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, contentItem))
            {
                return Forbid();
            }

            if (contentItem.Latest || contentItem.Published)
            {
                _notifier.Warning(H["The version to restore is already active."]);
                return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
            }

            await _auditTrailContentHandler.RestoringAsync(new RestoreContentContext(contentItem));

            var latest = await _contentManager.GetAsync(contentItem.ContentItemId, VersionOptions.Latest);
            if (latest != null)
            {
                latest.Latest = false;
                _session.Save(latest);
            }

            contentItem.Latest = true;
            _session.Save(contentItem);

            await _auditTrailContentHandler.RestoredAsync(new RestoreContentContext(contentItem));

            _notifier.Success(H["'{0}' has been restored.", contentItemToRestore.DisplayText]);
            return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
        }
    }
}
