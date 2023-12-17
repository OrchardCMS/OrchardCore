using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Secrets.Models;
using OrchardCore.Secrets.Options;
using OrchardCore.Secrets.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Secrets.Controllers;

[Admin]
public class AdminController : Controller
{
    private readonly ISecretService _secretService;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDisplayManager<SecretBase> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly SecretOptions _secretsOptions;
    private readonly ISiteService _siteService;
    private readonly INotifier _notifier;

    protected readonly dynamic New;
    protected readonly IStringLocalizer S;
    protected readonly IHtmlLocalizer H;

    public AdminController(
        ISecretService secretService,
        IAuthorizationService authorizationService,
        IDisplayManager<SecretBase> displayManager,
        IUpdateModelAccessor updateModelAccessor,
        IOptions<SecretOptions> secretsOptions,
        ISiteService siteService,
        INotifier notifier,
        IShapeFactory shapeFactory,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _secretService = secretService;
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
        _secretsOptions = secretsOptions.Value;
        _siteService = siteService;
        _notifier = notifier;
        New = shapeFactory;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(ContentOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSecrets))
        {
            return Forbid();
        }

        var siteSettings = await _siteService.GetSiteSettingsAsync();
        var pager = new Pager(pagerParameters, siteSettings.PageSize);

        var secretInfos = (await _secretService.GetSecretInfosAsync()).ToList();
        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            secretInfos = secretInfos.Where(kv => kv.Key.Contains(options.Search)).ToList();
        }

        var count = secretInfos.Count;

        secretInfos = secretInfos
            .OrderBy(kv => kv.Key)
            .Skip(pager.GetStartIndex())
            .Take(pager.PageSize)
            .ToList();

        var pagerShape = (await New.Pager(pager)).TotalItemCount(count);

        var thumbnails = new Dictionary<string, dynamic>();
        foreach (var type in _secretsOptions.SecretTypes)
        {
            var secret = _secretService.CreateSecret(type.Name);
            dynamic thumbnail = await _displayManager.BuildDisplayAsync(secret, _updateModelAccessor.ModelUpdater, "Thumbnail");
            thumbnail.Secret = secret;
            thumbnails.Add(type.Name, thumbnail);
        }

        var entries = new List<SecretInfoEntry>();
        foreach (var secretInfo in secretInfos)
        {
            var secret = await _secretService.GetSecretAsync(secretInfo.Value.Name);
            if (secret is null)
            {
                continue;
            }

            dynamic summary = await _displayManager.BuildDisplayAsync(secret, _updateModelAccessor.ModelUpdater, "Summary");
            summary.Secret = secret;
            entries.Add(new SecretInfoEntry
            {
                Name = secretInfo.Key,
                Info = secretInfo.Value,
                Summary = summary,
            });
        };

        var model = new SecretIndexViewModel
        {
            Entries = entries,
            Thumbnails = thumbnails,
            Options = options,
            Pager = pagerShape,
        };

        model.Options.ContentsBulkAction =
        [
            new() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) },
        ];

        return View("Index", model);
    }

    [HttpPost, ActionName("Index")]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> IndexPost(ContentOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSecrets))
        {
            return Forbid();
        }

        if (itemIds is not null && itemIds.Any())
        {
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var itemId in itemIds)
                    {
                        await _secretService.RemoveSecretAsync(itemId);
                    }

                    await _notifier.SuccessAsync(H["Secrets successfully removed."]);
                    break;
                default:
                    throw new InvalidOperationException($"Invalid bulk action '{options.BulkAction}'.");
            }
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Create(string type)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSecrets))
        {
            return Forbid();
        }

        var secret = _secretService.CreateSecret(type);
        if (secret is null)
        {
            return NotFound();
        }

        var model = new SecretInfoViewModel
        {
            Type = type,
            Editor = await _displayManager.BuildEditorAsync(secret, _updateModelAccessor.ModelUpdater, isNew: true, "", ""),
            StoreInfos = _secretService.GetSecretStoreInfos(),
        };

        return View(model);
    }

    [HttpPost, ActionName("Create")]
    public async Task<IActionResult> CreatePost(SecretInfoViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSecrets))
        {
            return Forbid();
        }

        var secret = _secretService.CreateSecret(model.Type);
        if (secret is null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(SecretInfoViewModel.Name), S["The name is mandatory."]);
            }

            if (!string.Equals(model.Name, model.Name.ToSafeSecretName(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(SecretInfoViewModel.Name), S["The name contains invalid characters."]);
            }

            var secretInfos = await _secretService.LoadSecretInfosAsync();

            // Do not check the stores as a readonly store would already have the key value.
            if (secretInfos.ContainsKey(model.Name))
            {
                ModelState.AddModelError(nameof(SecretInfoViewModel.Name), S["A secret with the same name already exists."]);
            }
        }

        dynamic editor = await _displayManager.UpdateEditorAsync(secret, updater: _updateModelAccessor.ModelUpdater, isNew: true, "", "");

        if (ModelState.IsValid)
        {
            var secretInfo = new SecretInfo
            {
                Name = model.Name,
                Store = model.SelectedStore,
                Description = model.Description,
                Type = model.Type,
            };

            await _secretService.UpdateSecretAsync(secretInfo, secret);
            await _notifier.SuccessAsync(H["Secret added successfully."]);

            return RedirectToAction(nameof(Index));
        }

        model.Editor = editor;
        model.StoreInfos = _secretService.GetSecretStoreInfos();

        // If we got this far, something failed, redisplay form.
        return View(model);
    }

    public async Task<IActionResult> Edit(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSecrets))
        {
            return Forbid();
        }

        var secretInfos = await _secretService.GetSecretInfosAsync();
        if (!secretInfos.TryGetValue(name, out var secretInfo))
        {
            return RedirectToAction(nameof(Create), new { name });
        }

        var secret = await _secretService.GetSecretAsync(secretInfo.Name);

        var model = new SecretInfoViewModel
        {
            Name = name,
            Description = secretInfo.Description,
            SelectedStore = secretInfo.Store,
            Type = secretInfo.Type,
            Editor = await _displayManager.BuildEditorAsync(secret, _updateModelAccessor.ModelUpdater, isNew: false, "", ""),
            StoreInfos = _secretService.GetSecretStoreInfos(),
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string sourceName, SecretInfoViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSecrets))
        {
            return Forbid();
        }

        var secretInfos = await _secretService.LoadSecretInfosAsync();
        if (ModelState.IsValid)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(SecretInfoViewModel.Name), S["The name is mandatory."]);
            }

            if (!model.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(model.Name, model.Name.ToSafeSecretName(), StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(SecretInfoViewModel.Name), S["The name contains invalid characters."]);
                }

                if (secretInfos.ContainsKey(model.Name))
                {
                    ModelState.AddModelError(nameof(SecretInfoViewModel.Name), S["A secret with the same name already exists."]);
                }
            }
        }

        var secret = await _secretService.GetSecretAsync(sourceName);
        if (secret is null)
        {
            return NotFound();
        }

        var editor = await _displayManager.UpdateEditorAsync(secret, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "");
        model.Editor = editor;

        if (ModelState.IsValid)
        {
            var secretInfo = new SecretInfo
            {
                Name = model.Name,
                Store = model.SelectedStore,
                Description = model.Description,
                Type = model.Type,
            };

            await _secretService.UpdateSecretAsync(secretInfo, secret, sourceName);

            return RedirectToAction(nameof(Index));
        }

        // If we got this far, something failed, redisplay form.
        model.StoreInfos = _secretService.GetSecretStoreInfos();

        // Prevent a page not found on the next post.
        model.Name = sourceName;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSecrets))
        {
            return Forbid();
        }

        if (!await _secretService.RemoveSecretAsync(name))
        {
            return NotFound();
        }

        await _notifier.SuccessAsync(H["Secret deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }
}
