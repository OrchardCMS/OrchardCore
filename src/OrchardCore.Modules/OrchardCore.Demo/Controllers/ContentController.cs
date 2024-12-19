using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.Contents;
using OrchardCore.DisplayManagement.ModelBinding;
using YesSql;
using IHttpContextAccessor = Microsoft.AspNetCore.Http.IHttpContextAccessor;

namespace OrchardCore.Demo.Controllers;

public sealed class ContentController : Controller
{
    private readonly IContentItemDisplayManager _contentDisplay;
    private readonly IContentManager _contentManager;
    private readonly ISession _session;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ContentController(
        IContentManager contentManager,
        IContentItemDisplayManager contentDisplay,
        ISession session,
        IUpdateModelAccessor updateModelAccessor,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _contentManager = contentManager;
        _contentDisplay = contentDisplay;
        _session = session;
        _updateModelAccessor = updateModelAccessor;
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ActionResult> Display(string contentItemId)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.ViewContent, contentItem))
        {
            return Forbid();
        }

        var shape = await _contentDisplay.BuildDisplayAsync(contentItem, _updateModelAccessor.ModelUpdater);
        return View(shape);
    }

    [Admin("Demo/Content/Edit", "Demo.Content.Edit")]
    public async Task<ActionResult> Edit(string contentItemId)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var shape = await _contentDisplay.BuildEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);
        return View(shape);
    }

    [Admin, HttpPost, ActionName("Edit")]
    public async Task<ActionResult> EditPost(string contentItemId)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, CommonPermissions.EditContent, contentItem))
        {
            return Forbid();
        }

        var shape = await _contentDisplay.UpdateEditorAsync(contentItem, _updateModelAccessor.ModelUpdater, false);

        if (!ModelState.IsValid)
        {
            await _session.CancelAsync();
            return View(nameof(Edit), shape);
        }

        await _session.SaveAsync(contentItem);
        return RedirectToAction(nameof(Edit), contentItemId);
    }
}
