using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.AuditTrail.Extensions;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Contents.AuditTrail.Indexes;
using OrchardCore.Contents.AuditTrail.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using YesSql;
using IYesSqlSession = YesSql.ISession;

namespace OrchardCore.Contents.AuditTrail.Controllers
{
    [RequireFeatures("OrchardCore.AuditTrail")]
    [Admin]
    public class ContentController : Controller
    {
        private readonly IHtmlLocalizer H;
        private readonly INotifier _notifier;
        private readonly IYesSqlSession _session;
        private readonly IContentManager _contentManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;

        public ContentController(
            IYesSqlSession session,
            INotifier notifier,
            IContentManager contentManager,
            IHttpContextAccessor httpContextAccessor,
            IUpdateModelAccessor updateModelAccessor,
            IAuthorizationService authorizationService,
            IHtmlLocalizer<ContentController> htmlLocalizer,
            IContentItemDisplayManager contentItemDisplayManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _session = session;
            _notifier = notifier;
            _contentManager = contentManager;
            _updateModelAccessor = updateModelAccessor;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;

            H = htmlLocalizer;
        }

        public async Task<ActionResult> Detail(int versionNumber, string auditTrailEventId)
        {
            var auditTrailEvent = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>()
                .Where(auditTrailEventIndex => auditTrailEventIndex.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync();

            // Create a new item to take into account the current type definition.
            var existing = auditTrailEvent.Get(auditTrailEvent.EventName).ToObject<ContentItem>();
            var contentItem = await _contentManager.NewAsync(existing.ContentType);
            contentItem.Merge(existing);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.EditContent, contentItem))
            {
                return Forbid();
            }

            contentItem = await _contentManager.LoadAsync(contentItem);

            var auditTrailPart = contentItem.As<AuditTrailPart>();

            if (auditTrailPart != null)
            {
                auditTrailPart.ShowComment = true;
            }

            dynamic contentItemEditor =
                await _contentItemDisplayManager.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);
            contentItemEditor.VersionNumber = versionNumber;

            return View(contentItemEditor);
        }

        [HttpPost]
        public async Task<ActionResult> Restore(string auditTrailEventId)
        {
            var auditTrailEventToRestore = await _session.Query<AuditTrailEvent, AuditTrailEventIndex>()
                .Where(auditTrailEventIndex => auditTrailEventIndex.AuditTrailEventId == auditTrailEventId)
                .FirstOrDefaultAsync();

            var contentItemToRestore = auditTrailEventToRestore.Get(auditTrailEventToRestore.EventName)
                .ToObject<ContentItem>();

            var auditTrailEvent = await _session.Query<AuditTrailEvent, ContentAuditTrailEventIndex>()
                .Where(eventIndex => eventIndex.ContentItemId == contentItemToRestore.ContentItemId &&
                    eventIndex.EventName != "Saved")
                .OrderByDescending(eventIndex => eventIndex.VersionNumber)
                .FirstOrDefaultAsync();

            // Create a new item to take into account the current type definition.
            var existing = auditTrailEvent.Get(auditTrailEvent.EventName).ToObject<ContentItem>();
            var contentItem = await _contentManager.NewAsync(existing.ContentType);
            contentItem.Merge(existing);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, contentItem))
            {
                return Forbid();
            }

            var activeVersions = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(contentItemIndex =>
                    contentItemIndex.ContentItemId == existing.ContentItemId &&
                    (contentItemIndex.Published || contentItemIndex.Latest)).ListAsync();

            foreach (var version in activeVersions)
            {
                version.Published = false;
                version.Latest = false;
                _session.Save(version);
            }

            // Adding this item to HttpContext.Features is necessary to be able to know that an earlier version of this
            // event has been restored, not a new one has been created.
            _httpContextAccessor.HttpContext.Features.Set(new AuditTrailContentItemRestoreFeature { ContentItem = contentItemToRestore });
            contentItemToRestore.Latest = true;
            await _contentManager.CreateAsync(contentItemToRestore, VersionOptions.Draft);

            _notifier.Success(H["{0} has been restored.", contentItemToRestore.DisplayText]);

            return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
        }
    }
}
