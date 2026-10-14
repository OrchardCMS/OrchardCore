using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Contents.ViewModels;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using YesSql;

namespace OrchardCore.Contents.Controllers;

/// <summary>
/// Creates several content items of the same type in a single request.
/// All writes use the current YesSql session so that a failed item cancels the batch.
/// </summary>
public sealed class BatchController(
    IContentManager contentManager,
    IContentItemDisplayManager contentItemDisplayManager,
    IContentDefinitionManager contentDefinitionManager,
    IAuthorizationService authorizationService,
    IUpdateModelAccessor updateModelAccessor,
    ISession session,
    INotifier notifier,
    IHtmlLocalizer<BatchController> localizer) : Controller
{
    internal const int MaxBatchSize = 5;

    [HttpGet]
    [Admin("Contents/ContentTypes/{id}/BatchCreate", "BatchCreateContentItems")]
    public async Task<IActionResult> BatchCreate(string id)
    {
        var error = await CheckAccessAsync(id, publish: false);
        if (error is not null)
        {
            return error;
        }

        var model = await BuildModelAsync(id, 1, update: false);
        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(BatchCreate))]
    public async Task<IActionResult> BatchCreatePost(string id, int count, string submit)
    {
        if (count is < 1 or > MaxBatchSize || submit is not ("Save" or "Publish"))
        {
            return BadRequest();
        }

        var publish = submit == "Publish";
        var error = await CheckAccessAsync(id, publish);
        if (error is not null)
        {
            return error;
        }

        // Update every editor before saving anything, so field validation can fail as a batch.
        var model = await BuildModelAsync(id, count, update: true);
        if (!ModelState.IsValid)
        {
            await session.CancelAsync();
            return View(nameof(BatchCreate), model);
        }

        // The session is committed by the Orchard request pipeline. If any item is rejected,
        // cancel it so that earlier items in this batch do not remain persisted.
        foreach (var item in model.ContentItems.Take(count))
        {
            if (!await contentManager.CreateAsync(item, VersionOptions.Draft))
            {
                ModelState.AddModelError(string.Empty, "One of the content items could not be created.");
                await session.CancelAsync();
                return View(nameof(BatchCreate), model);
            }

            if (publish)
            {
                if (!await contentManager.PublishAsync(item))
                {
                    ModelState.AddModelError(string.Empty, "One of the content items could not be published.");
                    await session.CancelAsync();
                    return View(nameof(BatchCreate), model);
                }
            }
            else
            {
                await contentManager.SaveDraftAsync(item);
            }
        }

        await notifier.SuccessAsync(publish
            ? localizer["{0} content items have been published.", count]
            : localizer["{0} content drafts have been saved.", count]);

        return RedirectToAction(nameof(BatchCreate), new { id });
    }

    private async Task<IActionResult> CheckAccessAsync(string id, bool publish)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        var definition = await contentDefinitionManager.GetTypeDefinitionAsync(id);
        if (definition is null || !definition.IsCreatable())
        {
            return NotFound();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!await authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.EditContent, id, userId)
            || (publish && !await authorizationService.AuthorizeContentTypeAsync(User, CommonPermissions.PublishContent, id, userId)))
        {
            return Forbid();
        }

        return null;
    }

    private async Task<BatchCreateContentViewModel> BuildModelAsync(string id, int count, bool update)
    {
        var definition = await contentDefinitionManager.GetTypeDefinitionAsync(id);
        var model = new BatchCreateContentViewModel
        {
            ContentType = id,
            ContentTypeDisplayName = definition?.DisplayName ?? id,
            Count = count,
        };

        for (var i = 0; i < MaxBatchSize; i++)
        {
            var item = await contentManager.NewAsync(id);
            var prefix = $"BatchItems[{i}]";
            var editor = update && i < count
                ? await contentItemDisplayManager.UpdateEditorAsync(item, updateModelAccessor.ModelUpdater, true, htmlFieldPrefix: prefix)
                : await contentItemDisplayManager.BuildEditorAsync(item, updateModelAccessor.ModelUpdater, true, htmlFieldPrefix: prefix);

            // Each item is edited separately; the batch form owns the save/publish buttons.
            editor.Properties["Actions"] = null;
            model.ContentItems.Add(item);
            model.Editors.Add(editor);
        }

        return model;
    }
}
