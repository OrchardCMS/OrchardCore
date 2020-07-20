using System.Linq;
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
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using YesSql;
using IYesSqlSession = YesSql.ISession;

namespace OrchardCore.AuditTrail.Controllers
{
    [Admin]
    public class ContentController : Controller
    {
        private readonly IHtmlLocalizer H;
        private readonly INotifier _notifier;
        private readonly IYesSqlSession _session;
        private readonly IHttpContextAccessor _hca;
        private readonly IContentManager _contentManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;


        public ContentController(
            IYesSqlSession session,
            INotifier notifier,
            IHttpContextAccessor hca,
            IContentManager contentManager,
            IUpdateModelAccessor updateModelAccessor,
            IAuthorizationService authorizationService,
            IHtmlLocalizer<ContentController> htmlLocalizer,
            IContentItemDisplayManager contentItemDisplayManager)
        {
            _hca = hca;
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

            var contentItem = auditTrailEvent.Get(auditTrailEvent.EventName).ToObject<ContentItem>();
           
            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
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

            var latestContentItem = auditTrailEvent.Get(auditTrailEvent.EventName).ToObject<ContentItem>();

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, latestContentItem))
            {
                return Forbid();
            }

            var activeVersions = await _session.Query<ContentItem, ContentItemIndex>()
                .Where(contentItemIndex =>
                    contentItemIndex.ContentItemId == latestContentItem.ContentItemId &&
                    (contentItemIndex.Published || contentItemIndex.Latest)).ListAsync();

            if (activeVersions.Any())
            {
                foreach (var version in activeVersions)
                {
                    version.Published = false;
                    version.Latest = false;
                    _session.Save(version);
                }
            }

            // Adding this item to HttpContenxt.Items is necessary to be able to know that an earlier version of this
            // event has been restored, not a new one has been created.
            _hca.HttpContext.Items.Add("OrchardCore.AuditTrail.Restored", contentItemToRestore);
            contentItemToRestore.Latest = true;
            await _contentManager.CreateAsync(contentItemToRestore, VersionOptions.Draft);

            _notifier.Information(H["{0} has been restored.", contentItemToRestore.DisplayText]);

            return RedirectToAction(nameof(AdminController.Index), "Admin");
        }
    }
}
