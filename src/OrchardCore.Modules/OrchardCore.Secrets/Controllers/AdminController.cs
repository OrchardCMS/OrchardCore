using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Navigation;
using OrchardCore.Routing;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Controllers;

[Admin("Secrets/{action}/{name?}", "Secrets{action}")]
public sealed class AdminController : Controller
{
    private const string OptionsSearch = "Options.Search";

    private readonly ISecretManager _secretManager;
    private readonly IEnumerable<ISecretTypeProvider> _secretTypeProviders;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly IShapeFactory _shapeFactory;
    private readonly PagerOptions _pagerOptions;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        ISecretManager secretManager,
        IEnumerable<ISecretTypeProvider> secretTypeProviders,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IShapeFactory shapeFactory,
        IOptions<PagerOptions> pagerOptions,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _secretManager = secretManager;
        _secretTypeProviders = secretTypeProviders;
        _authorizationService = authorizationService;
        _notifier = notifier;
        _shapeFactory = shapeFactory;
        _pagerOptions = pagerOptions.Value;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index(SecretIndexOptions options, PagerParameters pagerParameters)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ViewSecrets))
        {
            return Forbid();
        }

        options ??= new SecretIndexOptions();

        var providers = _secretTypeProviders.ToList();

        var secrets = (await _secretManager.GetSecretInfosAsync())
            .Select(info =>
            {
                var type = GetSimpleTypeName(info.Type);

                return new SecretEntryViewModel
                {
                    Name = info.Name,
                    Store = info.Store,
                    Type = type,
                    TypeDisplayName = providers.FirstOrDefault(p => p.Name.Equals(type, StringComparison.OrdinalIgnoreCase))?.DisplayName ?? type,
                    Description = info.Description,
                    CreatedUtc = info.CreatedUtc,
                    UpdatedUtc = info.UpdatedUtc,
                    ExpiresUtc = info.ExpiresUtc,
                };
            });

        if (!string.IsNullOrWhiteSpace(options.Search))
        {
            var search = options.Search.Trim();

            secrets = secrets.Where(secret =>
                Contains(secret.Name, search) ||
                Contains(secret.Description, search) ||
                Contains(secret.Store, search) ||
                Contains(secret.TypeDisplayName, search));
        }

        var filtered = secrets
            .OrderBy(secret => secret.Name, StringComparer.OrdinalIgnoreCase)
            .ThenBy(secret => secret.Store, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var pager = new Pager(pagerParameters, _pagerOptions);

        // Maintain previous route data when generating page links.
        var routeData = new RouteData();

        if (!string.IsNullOrEmpty(options.Search))
        {
            routeData.Values.TryAdd(OptionsSearch, options.Search);
        }

        options.BulkActions =
        [
            new SelectListItem(S["Delete"], nameof(SecretsBulkAction.Remove)),
        ];

        var model = new SecretIndexViewModel
        {
            Secrets = filtered.Skip(pager.GetStartIndex()).Take(pager.PageSize).ToList(),
            Options = options,
            Pager = await _shapeFactory.PagerAsync(pager, filtered.Count, routeData),
            AvailableTypes = providers.Select(p => new SecretTypeViewModel
            {
                Name = p.Name,
                DisplayName = p.DisplayName,
                Description = p.Description,
            }).ToList(),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.Filter")]
    public ActionResult IndexFilterPost(SecretIndexViewModel model)
        => RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { OptionsSearch, model.Options?.Search },
        });

    [HttpPost]
    [ActionName(nameof(Index))]
    [FormValueRequired("submit.BulkAction")]
    public async Task<IActionResult> IndexBulkActionPost(SecretIndexOptions options, IEnumerable<string> itemIds)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        if (itemIds?.Any() == true)
        {
            switch (options.BulkAction)
            {
                case SecretsBulkAction.None:
                    break;

                case SecretsBulkAction.Remove:
                    var infos = (await _secretManager.GetSecretInfosAsync()).ToList();
                    var removed = 0;

                    foreach (var itemId in itemIds)
                    {
                        if (!SecretEntryId.TryParse(itemId, out var store, out var name))
                        {
                            continue;
                        }

                        var info = infos.FirstOrDefault(info =>
                            info.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
                            info.Store.Equals(store, StringComparison.OrdinalIgnoreCase));

                        if (info is null)
                        {
                            continue;
                        }

                        await _secretManager.RemoveSecretAsync(info.Name, info.Store);
                        removed++;
                    }

                    await _notifier.SuccessAsync(H.Plural(removed, "1 secret was deleted.", "{0} secrets were deleted."));
                    break;

                default:
                    return BadRequest();
            }
        }

        return RedirectToAction(nameof(Index), new RouteValueDictionary
        {
            { OptionsSearch, options.Search },
        });
    }

    public async Task<IActionResult> Create(string type)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        // If no type specified, redirect to Index (type selection is handled via modal)
        if (string.IsNullOrEmpty(type))
        {
            return RedirectToAction(nameof(Index));
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
        model.Store = model.AvailableStores.FirstOrDefault();

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
        var infos = await _secretManager.GetSecretInfosAsync();
        if (infos.Any(info => info.Name.Equals(model.Name, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Name), S["A secret with this name already exists."]);
        }

        // Validate type-specific requirements
        ValidateSecretModel(model, true);

        if (ModelState.IsValid)
        {
            var secret = CreateSecretFromModel(model, null);
            var options = new SecretSaveOptions
            {
                Description = model.Description,
                ExpiresUtc = model.ExpiresUtc,
            };
            await SaveSecretAsync(model.Name, secret, model.Store, options);
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

    public async Task<IActionResult> Edit(string name, string store)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(name))
        {
            return NotFound();
        }

        var secretInfo = await FindSecretInfoAsync(name, store);

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
        var secret = await _secretManager.GetSecretAsync<ISecret>(secretInfo.Name, secretInfo.Store);
        if (secret == null)
        {
            return NotFound();
        }

        var model = new SecretEditViewModel
        {
            IsNew = false,
            Name = secretInfo.Name,
            Store = secretInfo.Store,
            SecretType = typeName,
            SecretTypeDisplayName = provider.DisplayName,
            ExpiresUtc = secretInfo.ExpiresUtc,
            Description = secretInfo.Description,
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
    public async Task<IActionResult> EditPost(SecretEditViewModel model, string name, [FromQuery] string store)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        var info = await FindSecretInfoAsync(name, store);
        if (info == null || !string.Equals(info.Store, model.Store, StringComparison.OrdinalIgnoreCase) ||
            GetSimpleTypeName(info.Type) != model.SecretType)
        {
            return BadRequest();
        }

        var typeName = GetSimpleTypeName(info.Type);
        var provider = _secretTypeProviders.FirstOrDefault(p => p.Name.Equals(typeName, StringComparison.OrdinalIgnoreCase));
        if (provider == null)
        {
            return NotFound();
        }

        // Get existing secret
        var existingSecret = await _secretManager.GetSecretAsync<ISecret>(info.Name, info.Store);
        if (existingSecret == null)
        {
            return NotFound();
        }

        // Validate type-specific requirements
        ValidateSecretModel(model, false);

        if (ModelState.IsValid)
        {
            var secret = CreateSecretFromModel(model, existingSecret);
            var options = new SecretSaveOptions
            {
                Description = model.Description,
                ExpiresUtc = model.ExpiresUtc,
            };
            await SaveSecretAsync(name, secret, model.Store, options);
            await _notifier.SuccessAsync(H["Secret updated successfully."]);
            return RedirectToAction(nameof(Index));
        }

        model.SecretTypeDisplayName = provider.DisplayName;
        model.IsNew = false;
        model.AvailableStores = _secretManager.GetStores()
            .Where(s => !s.IsReadOnly)
            .Select(s => s.Name)
            .ToList();

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string name, [FromQuery] string store)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        if (string.IsNullOrEmpty(name))
        {
            return NotFound();
        }

        var info = await FindSecretInfoAsync(name, store);
        if (info == null)
        {
            return NotFound();
        }

        await _secretManager.RemoveSecretAsync(info.Name, info.Store);
        await _notifier.SuccessAsync(H["Secret deleted successfully."]);

        return RedirectToAction(nameof(Index));
    }

    private void ValidateSecretModel(SecretEditViewModel model, bool isNew)
    {
        if (!_secretManager.GetStores().Any(s => !s.IsReadOnly && s.Name.Equals(model.Store, StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(nameof(model.Store), S["Select a writable secret store."]);
        }

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

    private async Task<SecretInfo> FindSecretInfoAsync(string name, string store)
    {
        var infos = await _secretManager.GetSecretInfosAsync();
        var matches = infos.Where(info => info.Name.Equals(name, StringComparison.OrdinalIgnoreCase) &&
            (string.IsNullOrEmpty(store) || info.Store.Equals(store, StringComparison.OrdinalIgnoreCase))).ToList();

        return matches.Count == 1 ? matches[0] : null;
    }

    private static void PopulateModelFromSecret(SecretEditViewModel model, ISecret secret)
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

    private static TextSecret CreateTextSecret(SecretEditViewModel model, TextSecret existing)
    {
        var secret = existing ?? new TextSecret();
        if (!string.IsNullOrEmpty(model.TextValue))
        {
            secret.Text = model.TextValue;
        }
        return secret;
    }

    private static RsaKeySecret CreateRsaKeySecret(SecretEditViewModel model, RsaKeySecret existing)
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

    private async Task SaveSecretAsync(string name, ISecret secret, string store, SecretSaveOptions options)
    {
        if (!string.IsNullOrEmpty(store))
        {
            await _secretManager.SaveSecretAsync(name, secret, store, options);
        }
        else
        {
            await _secretManager.SaveSecretAsync(name, secret, options);
        }
    }

    private static bool Contains(string value, string search)
        => value?.Contains(search, StringComparison.OrdinalIgnoreCase) == true;

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
