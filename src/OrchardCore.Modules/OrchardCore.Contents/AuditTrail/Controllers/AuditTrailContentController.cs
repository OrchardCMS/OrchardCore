using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.AuditTrail.Indexes;
using OrchardCore.AuditTrail.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
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
    public class AuditTrailContentController : Controller
    {
        private readonly ISession _session;
        private readonly IContentManager _contentManager;
        private readonly IUpdateModelAccessor _updateModelAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly INotifier _notifier;
        protected readonly IHtmlLocalizer H;
        private readonly ILogger _logger;

        public AuditTrailContentController(
            ISession session,
            IContentManager contentManager,
            IUpdateModelAccessor updateModelAccessor,
            IAuthorizationService authorizationService,
            IContentItemDisplayManager contentItemDisplayManager,
            INotifier notifier,
            IHtmlLocalizer<AuditTrailContentController> htmlLocalizer,
            ILogger<AuditTrailContentController> logger)
        {
            _session = session;
            _contentManager = contentManager;
            _updateModelAccessor = updateModelAccessor;
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _notifier = notifier;
            H = htmlLocalizer;
            _logger = logger;
        }

        public async Task<ActionResult> Display(string auditTrailEventId)
        {
            var auditTrailContentEvent = (await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index => index.EventId == auditTrailEventId)
                .FirstOrDefaultAsync())
                ?.As<AuditTrailContentEvent>();

            if (auditTrailContentEvent == null || auditTrailContentEvent.ContentItem == null)
            {
                return NotFound();
            }

            var contentItem = auditTrailContentEvent.ContentItem;

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

            model.Properties["VersionNumber"] = auditTrailContentEvent.VersionNumber;

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> Restore(string auditTrailEventId)
        {
            var contentItem = (await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
                .Where(index => index.EventId == auditTrailEventId)
                .FirstOrDefaultAsync())
                ?.As<AuditTrailContentEvent>()
                ?.ContentItem;

            if (contentItem == null)
            {
                return NotFound();
            }

            contentItem = await _contentManager.LoadAsync(contentItem);

            if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PublishContent, contentItem))
            {
                return Forbid();
            }

            var result = await _contentManager.RestoreAsync(contentItem);

            if (!result.Succeeded)
            {
                await _notifier.WarningAsync(H["'{0}' was not restored, the version is not valid.", contentItem.DisplayText]);

                foreach (var error in result.Errors)
                {
                    // Pass ErrorMessage as an argument to ensure it is encoded
                    await _notifier.WarningAsync(new LocalizedHtmlString(nameof(AuditTrailContentController.Restore), "{0}", false, error.ErrorMessage));
                }

                return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
            }

            await _notifier.SuccessAsync(H["'{0}' has been restored.", contentItem.DisplayText]);

            return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
        }
    }
}
