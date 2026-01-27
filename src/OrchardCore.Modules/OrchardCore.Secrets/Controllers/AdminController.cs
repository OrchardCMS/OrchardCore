using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Controllers;

[Admin("Secrets/{action}/{name?}", "Secrets{action}")]
public sealed class AdminController : Controller
{
    private readonly ISecretManager _secretManager;
    private readonly IEnumerable<ISecretTypeProvider> _secretTypeProviders;
    private readonly IDisplayManager<SecretBase> _secretDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISecretManager secretManager,
        IEnumerable<ISecretTypeProvider> secretTypeProviders,
        IDisplayManager<SecretBase> secretDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _secretManager = secretManager;
        _secretTypeProviders = secretTypeProviders;
        _secretDisplayManager = secretDisplayManager;
        _updateModelAccessor = updateModelAccessor;
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

    public async Task<IActionResult> Create(string type)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        // If no type specified, show type selection
        if (string.IsNullOrEmpty(type))
        {
            var typeSelectionModel = new SecretTypeSelectionViewModel
            {
                AvailableTypes = _secretTypeProviders.Select(p => new SecretTypeViewModel
                {
                    Name = p.Name,
                    DisplayName = p.DisplayName,
                    Description = p.Description,
                }).ToList(),
            };

            return View("SelectType", typeSelectionModel);
        }

        // Find the provider for this type
        var provider = _secretTypeProviders.FirstOrDefault(p => p.Name.Equals(type, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            return NotFound();
        }

        // Create a new secret instance and build the editor
        var secret = (SecretBase)provider.Create();
        var shape = await _secretDisplayManager.BuildEditorAsync(secret, _updateModelAccessor.ModelUpdater, isNew: true);

        var model = new SecretEditViewModel
        {
            IsNew = true,
            SecretType = provider.Name,
            SecretTypeDisplayName = provider.DisplayName,
            AvailableStores = _secretManager.GetStores()
                .Where(s => !s.IsReadOnly)
                .Select(s => s.Name)
                .ToList(),
            Editor = shape,
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

        // Find the provider for this type
        var provider = _secretTypeProviders.FirstOrDefault(p => p.Name.Equals(model.SecretType, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            return NotFound();
        }

        // Check if secret already exists
        var existingSecret = await _secretManager.GetSecretAsync<ISecret>(model.Name);
        if (existingSecret != null)
        {
            ModelState.AddModelError(nameof(model.Name), S["A secret with this name already exists."]);
        }

        // Create new secret and update from form
        var secret = (SecretBase)provider.Create();
        var shape = await _secretDisplayManager.UpdateEditorAsync(secret, _updateModelAccessor.ModelUpdater, isNew: true);

        if (ModelState.IsValid)
        {
            await SaveSecretAsync(model.Name, secret, model.Store);
            await _notifier.SuccessAsync(H["Secret created successfully."]);
            return RedirectToAction(nameof(Index));
        }

        model.IsNew = true;
        model.SecretTypeDisplayName = provider.DisplayName;
        model.AvailableStores = _secretManager.GetStores()
            .Where(s => !s.IsReadOnly)
            .Select(s => s.Name)
            .ToList();
        model.Editor = shape;

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

        var typeName = GetSimpleTypeName(secretInfo.Type);
        var provider = _secretTypeProviders.FirstOrDefault(p => p.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            return NotFound();
        }

        // Get the actual secret to build the editor
        var secret = await _secretManager.GetSecretAsync<SecretBase>(name);
        if (secret == null)
        {
            secret = (SecretBase)provider.Create();
        }

        var shape = await _secretDisplayManager.BuildEditorAsync(secret, _updateModelAccessor.ModelUpdater, isNew: false);

        var model = new SecretEditViewModel
        {
            IsNew = false,
            Name = secretInfo.Name,
            Store = secretInfo.Store,
            SecretType = typeName,
            SecretTypeDisplayName = provider.DisplayName,
            AvailableStores = _secretManager.GetStores()
                .Where(s => !s.IsReadOnly)
                .Select(s => s.Name)
                .ToList(),
            Editor = shape,
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Edit))]
    public async Task<IActionResult> EditPost(SecretEditViewModel model, string name)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        var typeName = model.SecretType;
        var provider = _secretTypeProviders.FirstOrDefault(p => p.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            return NotFound();
        }

        // Get existing secret or create new one
        var secret = await _secretManager.GetSecretAsync<SecretBase>(name);
        if (secret == null)
        {
            secret = (SecretBase)provider.Create();
        }

        var shape = await _secretDisplayManager.UpdateEditorAsync(secret, _updateModelAccessor.ModelUpdater, isNew: false);

        if (ModelState.IsValid)
        {
            await SaveSecretAsync(name, secret, model.Store);
            await _notifier.SuccessAsync(H["Secret updated successfully."]);
            return RedirectToAction(nameof(Index));
        }

        model.SecretTypeDisplayName = provider.DisplayName;
        model.AvailableStores = _secretManager.GetStores()
            .Where(s => !s.IsReadOnly)
            .Select(s => s.Name)
            .ToList();
        model.Editor = shape;

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

    private async Task SaveSecretAsync(string name, ISecret secret, string store)
    {
        if (!string.IsNullOrEmpty(store))
        {
            await _secretManager.SaveSecretAsync(name, secret, store);
        }
        else
        {
            await _secretManager.SaveSecretAsync(name, secret);
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
