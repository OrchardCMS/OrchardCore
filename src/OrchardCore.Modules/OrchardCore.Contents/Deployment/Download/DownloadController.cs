using System.Net.Mime;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.Deployment;
using OrchardCore.Modules;

namespace OrchardCore.Contents.Deployment.Download;

[Admin("Download/{action}/{contentItemId}", AdminAttribute.NameFromControllerAndAction)]
[Feature("OrchardCore.Contents.Deployment.Download")]
public sealed class DownloadController : Controller
{
    private readonly ContentExportService _exports;

    public DownloadController(
        IAuthorizationService authorizationService,
        IContentManager contentManager)
    {
        _exports = new ContentExportService(contentManager, authorizationService);
    }

    [HttpGet]
    public async Task<IActionResult> Display(string contentItemId, bool latest = false)
    {
        ContentItem contentItem;
        try { contentItem = await _exports.GetAsync(contentItemId, latest, User); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        if (contentItem is null) { return NotFound(); }

        var model = new DisplayJsonContentItemViewModel
        {
            ContentItem = contentItem,
            ContentItemJson = ContentExportService.Serialize(contentItem).ToString(),
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Download(string contentItemId, bool latest = false)
    {
        ContentItem contentItem;
        try { contentItem = await _exports.GetAsync(contentItemId, latest, User); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        if (contentItem is null) { return NotFound(); }

        var jItem = ContentExportService.Serialize(contentItem);

        return File(Encoding.UTF8.GetBytes(jItem.ToString()), MediaTypeNames.Application.Json, $"{contentItem.ContentItemId}.json");
    }
}
