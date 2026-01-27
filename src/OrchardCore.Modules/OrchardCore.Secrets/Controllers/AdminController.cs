using System.Security.Cryptography;
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
    private readonly IEnumerable<ISecretTypeProvider> _secretTypeProviders;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISecretManager secretManager,
        IEnumerable<ISecretTypeProvider> secretTypeProviders,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _secretManager = secretManager;
        _secretTypeProviders = secretTypeProviders;
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
                Type = GetSimpleTypeName(info.Type),
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

        var model = new SecretEditViewModel
        {
            IsNew = true,
            SecretType = provider.Name,
            SecretTypeDisplayName = provider.DisplayName,
            AvailableStores = _secretManager.GetStores()
                .Where(s => !s.IsReadOnly)
                .Select(s => s.Name)
                .ToList(),
            // Set defaults for X509Secret
            X509StoreLocation = "CurrentUser",
            X509StoreName = "My",
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

        // Validate type-specific requirements
        ValidateSecretModel(model, true);

        if (ModelState.IsValid)
        {
            var secret = CreateSecretFromModel(model, null);
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

        // Get the actual secret to populate form values
        var secret = await _secretManager.GetSecretAsync<ISecret>(name);

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
        };

        // Populate type-specific fields from secret
        PopulateModelFromSecret(model, secret);

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

        // Get existing secret
        var existingSecret = await _secretManager.GetSecretAsync<ISecret>(name);

        // Validate type-specific requirements
        ValidateSecretModel(model, false);

        if (ModelState.IsValid)
        {
            var secret = CreateSecretFromModel(model, existingSecret);
            await SaveSecretAsync(name, secret, model.Store);
            await _notifier.SuccessAsync(H["Secret updated successfully."]);
            return RedirectToAction(nameof(Index));
        }

        model.SecretTypeDisplayName = provider.DisplayName;
        model.AvailableStores = _secretManager.GetStores()
            .Where(s => !s.IsReadOnly)
            .Select(s => s.Name)
            .ToList();

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

    private void ValidateSecretModel(SecretEditViewModel model, bool isNew)
    {
        switch (model.SecretType)
        {
            case nameof(TextSecret):
                if (isNew && string.IsNullOrEmpty(model.TextValue))
                {
                    ModelState.AddModelError(nameof(model.TextValue), S["The secret value is required."]);
                }
                break;

            case nameof(X509Secret):
                if (string.IsNullOrEmpty(model.X509Thumbprint))
                {
                    ModelState.AddModelError(nameof(model.X509Thumbprint), S["The certificate thumbprint is required."]);
                }
                break;
        }
    }

    private void PopulateModelFromSecret(SecretEditViewModel model, ISecret secret)
    {
        if (secret is X509Secret x509Secret)
        {
            model.X509StoreLocation = x509Secret.StoreLocation.ToString();
            model.X509StoreName = x509Secret.StoreName.ToString();
            model.X509Thumbprint = x509Secret.Thumbprint;
        }
        // TextSecret: don't expose the value
        // RsaKeySecret: don't expose the private key
    }

    private ISecret CreateSecretFromModel(SecretEditViewModel model, ISecret existingSecret)
    {
        return model.SecretType switch
        {
            nameof(TextSecret) => CreateTextSecret(model, existingSecret as TextSecret),
            nameof(RsaKeySecret) => CreateRsaKeySecret(model, existingSecret as RsaKeySecret),
            nameof(X509Secret) => CreateX509Secret(model),
            _ => throw new InvalidOperationException($"Unknown secret type: {model.SecretType}"),
        };
    }

    private TextSecret CreateTextSecret(SecretEditViewModel model, TextSecret existing)
    {
        var secret = existing ?? new TextSecret();
        if (!string.IsNullOrEmpty(model.TextValue))
        {
            secret.Text = model.TextValue;
        }
        return secret;
    }

    private RsaKeySecret CreateRsaKeySecret(SecretEditViewModel model, RsaKeySecret existing)
    {
        if (existing != null)
        {
            // Keep existing keys, they can't be modified
            return existing;
        }

        // Generate new RSA key pair
        using var rsa = RSA.Create(model.RsaKeySize > 0 ? model.RsaKeySize : 2048);
        return new RsaKeySecret
        {
            PublicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey()),
            PrivateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey()),
        };
    }

    private static X509Secret CreateX509Secret(SecretEditViewModel model)
    {
        var storeLocation = Enum.TryParse<System.Security.Cryptography.X509Certificates.StoreLocation>(
            model.X509StoreLocation, out var loc) ? loc : System.Security.Cryptography.X509Certificates.StoreLocation.CurrentUser;
        var storeName = Enum.TryParse<System.Security.Cryptography.X509Certificates.StoreName>(
            model.X509StoreName, out var name) ? name : System.Security.Cryptography.X509Certificates.StoreName.My;

        return new X509Secret
        {
            StoreLocation = storeLocation,
            StoreName = storeName,
            Thumbprint = model.X509Thumbprint,
        };
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
