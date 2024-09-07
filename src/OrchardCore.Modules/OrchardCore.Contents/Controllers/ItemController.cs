using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents.Controllers;

public sealed class ItemController : Controller, IUpdateModel
{
    private readonly IContentManager _contentManager;
    private readonly IContentItemDisplayManager _contentItemDisplayManager;
    private readonly IAuthorizationService _authorizationService;

    public ItemController(
        IContentManager contentManager,
        IContentItemDisplayManager contentItemDisplayManager,
        IAuthorizationService authorizationService)
    {
        _contentManager = contentManager;
        _contentItemDisplayManager = contentItemDisplayManager;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> Display(string contentItemId, string jsonPath)
    {
        var contentItem = await _contentManager.GetAsync(contentItemId, jsonPath);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.ViewContent, contentItem))
        {
            return this.ChallengeOrForbid();
        }

        var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

        return View(model);
    }

    public async Task<IActionResult> Preview(string contentItemId)
    {
        if (contentItemId == null)
        {
            return NotFound();
        }

        var versionOptions = VersionOptions.Latest;

        var contentItem = await _contentManager.GetAsync(contentItemId, versionOptions);

        if (contentItem == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.PreviewContent, contentItem))
        {
            return this.ChallengeOrForbid();
        }

        var model = await _contentItemDisplayManager.BuildDisplayAsync(contentItem, this);

        return View(model);
    }
}
