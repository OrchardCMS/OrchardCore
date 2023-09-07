using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Mvc.Utilities;
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
    private readonly IAuthorizationService _authorizationService;
    private readonly ISecretService _secretService;
    private readonly IDisplayManager<SecretBase> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly SecretOptions _secretsOptions;
    private readonly ISiteService _siteService;
    private readonly INotifier _notifier;

    protected readonly dynamic New;
    protected readonly IStringLocalizer S;
    protected readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        ISecretService secretService,
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

        var secretBindings = (await _secretService.GetSecretBindingsAsync()).ToList();
        if (!String.IsNullOrWhiteSpace(options.Search))
        {
            secretBindings = secretBindings.Where(kv => kv.Key.Contains(options.Search)).ToList();
        }

        var count = secretBindings.Count;

        secretBindings = secretBindings
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

        var entries = new List<SecretBindingEntry>();
        foreach (var binding in secretBindings)
        {
            var secret = await _secretService.GetSecretAsync(binding.Value);
            if (secret is null)
            {
                continue;
            }

            dynamic summary = await _displayManager.BuildDisplayAsync(secret, _updateModelAccessor.ModelUpdater, "Summary");
            summary.Secret = secret;
            entries.Add(new SecretBindingEntry
            {
                Name = binding.Key,
                SecretBinding = binding.Value,
                Summary = summary,
            });
        };

        var model = new SecretBindingIndexViewModel
        {
            Entries = entries,
            Thumbnails = thumbnails,
            Options = options,
            Pager = pagerShape,
        };

        model.Options.ContentsBulkAction = new List<SelectListItem>()
        {
            new SelectListItem() { Text = S["Delete"], Value = nameof(ContentsBulkAction.Remove) },
        };

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
            var secretBindings = await _secretService.GetSecretBindingsAsync();
            var checkedSecretBindings = secretBindings.Where(kv => itemIds.Contains(kv.Key));
            switch (options.BulkAction)
            {
                case ContentsBulkAction.None:
                    break;
                case ContentsBulkAction.Remove:
                    foreach (var binding in checkedSecretBindings)
                    {
                        await _secretService.RemoveSecretAsync(binding.Value);
                    }

                    await _notifier.SuccessAsync(H["Secrets successfully removed."]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(options.BulkAction), "Invalid bulk action.");
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

        var model = new SecretBindingViewModel
        {
            Type = type,
            Editor = await _displayManager.BuildEditorAsync(secret, _updateModelAccessor.ModelUpdater, isNew: true, "", ""),
            StoreInfos = _secretService.GetSecretStoreInfos(),
            Secret = secret,
        };

        model.Editor.Secret = secret;

        return View(model);
    }

    [HttpPost, ActionName("Create")]
    public async Task<IActionResult> CreatePost(SecretBindingViewModel model)
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
            if (String.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(SecretBindingViewModel.Name), S["The name is mandatory."]);
            }

            if (!String.Equals(model.Name, model.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(SecretBindingViewModel.Name), S["The name contains invalid characters."]);
            }

            var secretBindings = await _secretService.LoadSecretBindingsAsync();

            // Do not check the stores as a readonly store would already have the key value.
            if (secretBindings.ContainsKey(model.Name))
            {
                ModelState.AddModelError(nameof(SecretBindingViewModel.Name), S["A secret with the same name already exists."]);
            }
        }

        dynamic editor = await _displayManager.UpdateEditorAsync(secret, updater: _updateModelAccessor.ModelUpdater, isNew: true, "", "");
        editor.Secret = secret;

        if (ModelState.IsValid)
        {
            var binding = new SecretBinding
            {
                Name = model.Name,
                Store = model.SelectedStore,
                Description = model.Description,
                Type = model.Type,
            };

            await _secretService.UpdateSecretAsync(binding, secret);
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

        var secretBindings = await _secretService.GetSecretBindingsAsync();
        if (!secretBindings.TryGetValue(name, out var binding))
        {
            return RedirectToAction(nameof(Create), new { name });
        }

        var secret = await _secretService.GetSecretAsync(binding);

        var model = new SecretBindingViewModel
        {
            Name = name,
            SelectedStore = binding.Store,
            Description = binding.Description,
            Type = binding.Type,
            Editor = await _displayManager.BuildEditorAsync(secret, _updateModelAccessor.ModelUpdater, isNew: false, "", ""),
            StoreInfos = _secretService.GetSecretStoreInfos(),
            Secret = secret,
        };

        model.Editor.Secret = secret;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(string sourceName, SecretBindingViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageSecrets))
        {
            return Forbid();
        }

        var secretBindings = await _secretService.LoadSecretBindingsAsync();

        if (ModelState.IsValid)
        {
            if (String.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(SecretBindingViewModel.Name), S["The name is mandatory."]);
            }
            if (!model.Name.Equals(sourceName, StringComparison.OrdinalIgnoreCase))
            {
                if (!String.Equals(model.Name, model.Name.ToSafeName(), StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(SecretBindingViewModel.Name), S["The name contains invalid characters."]);
                }
                if (secretBindings.ContainsKey(model.Name))
                {
                    ModelState.AddModelError(nameof(SecretBindingViewModel.Name), S["A secret with the same name already exists."]);
                }
            }
        }

        if (!secretBindings.TryGetValue(sourceName, out var binding))
        {
            return NotFound();
        }

        var secret = await _secretService.GetSecretAsync(binding);

        var editor = await _displayManager.UpdateEditorAsync(secret, updater: _updateModelAccessor.ModelUpdater, isNew: false, "", "");
        model.Editor = editor;

        if (ModelState.IsValid)
        {
            // Remove this before updating the binding value.
            await _secretService.RemoveSecretAsync(binding);

            binding.Name = model.Name;
            binding.Store = model.SelectedStore;
            binding.Description = model.Description;

            await _secretService.UpdateSecretAsync(binding, secret);

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

        var secretBindings = await _secretService.GetSecretBindingsAsync();
        if (!secretBindings.TryGetValue(name, out var binding))
        {
            return NotFound();
        }

        await _secretService.RemoveSecretAsync(binding);

        await _notifier.SuccessAsync(H["Secret deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }
}
