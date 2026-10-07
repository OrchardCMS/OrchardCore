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

namespace OrchardCore.Contents.AuditTrail.Controllers;

[RequireFeatures("OrchardCore.AuditTrail")]
[Admin("AuditTrail/Content/{action}/{auditTrailEventId}", "{action}AuditTrailContent")]
public sealed class AuditTrailContentController : Controller
{
    private readonly ISession _session;
    private readonly IContentManager _contentManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;

    public AuditTrailContentController(
        ISession session,
        IContentManager contentManager,
        IUpdateModelAccessor updateModelAccessor,
        IAuthorizationService authorizationService,
        IContentItemDisplayManager contentItemDisplayManager,
        INotifier notifier,
        IHtmlLocalizer<AuditTrailContentController> htmlLocalizer)
    {
        _session = session;
        _contentManager = contentManager;
        _updateModelAccessor = updateModelAccessor;
        _authorizationService = authorizationService;
        _contentItemDisplayManager = contentItemDisplayManager;
        _notifier = notifier;
        H = htmlLocalizer;
    }

    public async Task<ActionResult> Display(string auditTrailEventId)
    {
        var auditTrailContentEvent = (await _session.Query<AuditTrailEvent, AuditTrailEventIndex>(collection: AuditTrailEvent.Collection)
            .Where(index => index.EventId == auditTrailEventId)
            .FirstOrDefaultAsync())
            ?.GetOrCreate<AuditTrailContentEvent>();

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

        if (contentItem.TryGet<AuditTrailPart>(out var auditTrailPart))
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
            ?.GetOrCreate<AuditTrailContentEvent>()
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
            var errorMessages = result.Errors.Select(error => error.ErrorMessage).ToArray();
            if (errorMessages.Length > 0)
            {
                await _notifier.WarningAsync(H.Plural(errorMessages.Length, "'{1}' was not restored, the version is not valid. {2}", "'{1}' was not restored, the version is not valid. Errors: {2}", contentItem.DisplayText, string.Join(", ", errorMessages)));
            }
            else
            {
                await _notifier.WarningAsync(H["'{0}' was not restored, the version is not valid.", contentItem.DisplayText]);
            }

            return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
        }

        await _notifier.SuccessAsync(H["'{0}' has been restored.", contentItem.DisplayText]);

        return RedirectToAction("Index", "Admin", new { area = "OrchardCore.AuditTrail" });
    }
}
