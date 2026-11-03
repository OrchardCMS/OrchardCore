using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Controllers;

[Admin("Secrets/Stores/{action}", "SecretStores{action}")]
public sealed class StoreController : Controller
{
    private readonly ISecretManager _manager;
    private readonly ISecretStoreOperations _operations;
    private readonly IAuthorizationService _authorization;
    private readonly ILogger _logger;
    private readonly IStringLocalizer S;

    public StoreController(
        ISecretManager manager,
        ISecretStoreOperations operations,
        IAuthorizationService authorization,
        ILogger<StoreController> logger,
        IStringLocalizer<StoreController> localizer)
    {
        _manager = manager;
        _operations = operations;
        _authorization = authorization;
        _logger = logger;
        S = localizer;
    }

    public async Task<IActionResult> Index(string sourceStore, string name)
    {
        if (!await _authorization.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        var model = new SecretStoreViewModel { SourceStore = sourceStore, Name = name };
        await LoadAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Index))]
    public async Task<IActionResult> IndexPost(SecretStoreViewModel model)
    {
        if (!await _authorization.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        var source = _manager.GetStores().FirstOrDefault(s => !s.IsReadOnly &&
            s.Name.Equals(model.SourceStore, StringComparison.OrdinalIgnoreCase));
        if (source == null)
        {
            ModelState.AddModelError(nameof(model.SourceStore), S["Select an available writable source store."]);
        }

        if (!model.Confirm)
        {
            ModelState.AddModelError(nameof(model.Confirm), S["Confirm the operation before continuing."]);
        }

        if (!_manager.GetStores().Any(s => !s.IsReadOnly && s.Name.Equals(model.DestinationStore, StringComparison.OrdinalIgnoreCase) && s != source))
        {
            ModelState.AddModelError(nameof(model.DestinationStore), S["Select a different writable destination store."]);
        }

        if (ModelState.IsValid)
        {
            try
            {
                model.Results = !string.IsNullOrEmpty(model.Name)
                    ? [await _operations.MoveSecretAsync(model.Name, model.SourceStore, model.DestinationStore)]
                    : await _operations.MoveAllSecretsAsync(model.SourceStore, model.DestinationStore);
                if (model.Results.Any(result => !result.Succeeded))
                {
                    ModelState.AddModelError(string.Empty, S["Some secrets could not be processed. Review the individual results before retrying or disabling the store."]);
                }
            }
            catch (Exception exception)
            {
                LogFailure(exception);
                ModelState.AddModelError(string.Empty, S["The store operation could not be completed. Check store availability, permissions, and whether another operation is in progress."]);
            }
        }

        if (model.Confirm)
        {
            model.Confirm = false;
            ModelState.Remove(nameof(model.Confirm));
        }

        await LoadAsync(model);
        return View(nameof(Index), model);
    }

    private async Task LoadAsync(SecretStoreViewModel model)
    {
        model.ActiveCount = null;
        model.AvailableStores = _manager.GetStores().Where(s => !s.IsReadOnly).Select(s => s.Name).ToList();
        model.SourceStore ??= model.AvailableStores.FirstOrDefault();
        var source = _manager.GetStores().FirstOrDefault(s => !s.IsReadOnly && s.Name.Equals(model.SourceStore, StringComparison.OrdinalIgnoreCase));
        if (source == null)
        {
            ModelState.AddModelError(nameof(model.SourceStore), S["Select an available writable source store."]);
            return;
        }

        try
        {
            model.ActiveCount = (await source.GetSecretInfosAsync()).Count();
        }
        catch (Exception exception)
        {
            LogFailure(exception);
            ModelState.AddModelError(string.Empty, S["The store could not be inspected. Its cleanup status is unknown; do not disable it based on this page."]);
        }
    }

    private void LogFailure(Exception exception)
    {
        if (_logger.IsEnabled(LogLevel.Error))
        {
            _logger.LogError("Secret store management failed ({ExceptionType}). No storage response body is logged.", exception.GetType().Name);
        }
    }
}
