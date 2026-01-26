using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Controllers;

[Admin("Secrets/{action}/{name?}", "Secrets{action}")]
public sealed class AdminController : Controller
{
    private readonly ISecretManager _secretManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISecretManager secretManager,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _secretManager = secretManager;
        _authorizationService = authorizationService;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ViewSecrets))
        {
            return Forbid();
        }

        var secretInfos = await _secretManager.GetSecretInfosAsync();

        var model = new SecretIndexViewModel
        {
            Secrets = secretInfos.Select(info => new SecretEntryViewModel
            {
                Name = info.Name,
                Store = info.Store,
                Type = info.Type,
                CreatedUtc = info.CreatedUtc,
                UpdatedUtc = info.UpdatedUtc,
            }).ToList(),
        };

        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        var model = new SecretEditViewModel
        {
            IsNew = true,
            AvailableStores = _secretManager.GetStores()
                .Where(s => !s.IsReadOnly)
                .Select(s => s.Name)
                .ToList(),
            AvailableTypes =
            [
                nameof(TextSecret),
            ],
        };

        return View(nameof(Edit), model);
    }

    [HttpPost]
    [ActionName(nameof(Create))]
    public async Task<IActionResult> CreatePost(SecretEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var existingSecret = await _secretManager.GetSecretAsync<ISecret>(model.Name);
            if (existingSecret != null)
            {
                ModelState.AddModelError(nameof(model.Name), S["A secret with this name already exists."]);
            }
            else
            {
                await SaveSecretAsync(model);
                await _notifier.SuccessAsync(H["Secret created successfully."]);
                return RedirectToAction(nameof(Index));
            }
        }

        model.IsNew = true;
        model.AvailableStores = _secretManager.GetStores()
            .Where(s => !s.IsReadOnly)
            .Select(s => s.Name)
            .ToList();
        model.AvailableTypes = [nameof(TextSecret)];

        return View(nameof(Edit), model);
    }

    public async Task<IActionResult> Edit(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(name))
        {
            return NotFound();
        }

        var secretInfos = await _secretManager.GetSecretInfosAsync();
        var secretInfo = secretInfos.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (secretInfo == null)
        {
            return NotFound();
        }

        var model = new SecretEditViewModel
        {
            IsNew = false,
            Name = secretInfo.Name,
            Store = secretInfo.Store,
            SecretType = GetSimpleTypeName(secretInfo.Type),
            AvailableStores = _secretManager.GetStores()
                .Where(s => !s.IsReadOnly)
                .Select(s => s.Name)
                .ToList(),
            AvailableTypes = [nameof(TextSecret)],
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(SecretEditViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            await SaveSecretAsync(model);
            await _notifier.SuccessAsync(H["Secret updated successfully."]);
            return RedirectToAction(nameof(Index));
        }

        model.AvailableStores = _secretManager.GetStores()
            .Where(s => !s.IsReadOnly)
            .Select(s => s.Name)
            .ToList();
        model.AvailableTypes = [nameof(TextSecret)];

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(name))
        {
            return NotFound();
        }

        await _secretManager.RemoveSecretAsync(name);
        await _notifier.SuccessAsync(H["Secret deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    private async Task SaveSecretAsync(SecretEditViewModel model)
    {
        switch (model.SecretType)
        {
            case nameof(TextSecret):
                var textSecret = new TextSecret { Text = model.SecretValue };
                if (!string.IsNullOrEmpty(model.Store))
                {
                    await _secretManager.SaveSecretAsync(model.Name, textSecret, model.Store);
                }
                else
                {
                    await _secretManager.SaveSecretAsync(model.Name, textSecret);
                }
                break;
            default:
                throw new InvalidOperationException($"Unknown secret type: {model.SecretType}");
        }
    }

    private static string GetSimpleTypeName(string fullTypeName)
    {
        if (string.IsNullOrEmpty(fullTypeName))
        {
            return nameof(TextSecret);
        }

        var lastDot = fullTypeName.LastIndexOf('.');
        return lastDot >= 0 ? fullTypeName[(lastDot + 1)..] : fullTypeName;
    }
}
