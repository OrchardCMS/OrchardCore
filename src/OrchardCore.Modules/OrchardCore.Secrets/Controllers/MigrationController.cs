using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Shell;
using OrchardCore.Secrets.ViewModels;

namespace OrchardCore.Secrets.Controllers;

/// <summary>
/// Moves credentials kept in the settings of other features to secrets. Each feature contributes the credentials it
/// keeps through a display driver of <see cref="SecretMigration"/>.
/// </summary>
[Admin("Secrets/Migration/{action}", "SecretsMigration{action}")]
public sealed class MigrationController : Controller
{
    private readonly IDisplayManager<SecretMigration> _displayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly ISecretManager _secretManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public MigrationController(
        IDisplayManager<SecretMigration> displayManager,
        IUpdateModelAccessor updateModelAccessor,
        ISecretManager secretManager,
        IAuthorizationService authorizationService,
        IShellReleaseManager shellReleaseManager,
        INotifier notifier,
        IStringLocalizer<MigrationController> stringLocalizer,
        IHtmlLocalizer<MigrationController> htmlLocalizer)
    {
        _displayManager = displayManager;
        _updateModelAccessor = updateModelAccessor;
        _secretManager = secretManager;
        _authorizationService = authorizationService;
        _shellReleaseManager = shellReleaseManager;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public async Task<IActionResult> Index()
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        var model = new SecretMigrationViewModel
        {
            AvailableStores = GetWritableStores(),
        };

        model.Store = model.AvailableStores.FirstOrDefault();
        model.Editor = await _displayManager.BuildEditorAsync(new SecretMigration { Store = model.Store }, _updateModelAccessor.ModelUpdater, false);

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    public async Task<IActionResult> IndexPost(SecretMigrationViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SecretsPermissions.ManageSecrets))
        {
            return Forbid();
        }

        model.AvailableStores = GetWritableStores();

        if (!model.AvailableStores.Contains(model.Store, StringComparer.OrdinalIgnoreCase))
        {
            ModelState.AddModelError(nameof(model.Store), S["Select a writable secret store."]);
        }

        var migration = new SecretMigration
        {
            Store = model.Store,
        };

        if (!ModelState.IsValid)
        {
            model.Editor = await _displayManager.BuildEditorAsync(migration, _updateModelAccessor.ModelUpdater, false);

            return View(nameof(Index), model);
        }

        await _displayManager.UpdateEditorAsync(migration, _updateModelAccessor.ModelUpdater, false);

        // The settings are updated once every secret is saved, see SecretMigration.UpdateSettingsAsync().
        await migration.UpdateSettingsAsync();
        model.Results = migration.Results;

        if (migration.Results.Count == 0)
        {
            await _notifier.WarningAsync(H["Select at least one credential to move."]);
            model.Editor = await _displayManager.BuildEditorAsync(migration, _updateModelAccessor.ModelUpdater, false);

            return View(nameof(Index), model);
        }

        var moved = migration.Results.Count(result => result.Succeeded);

        if (moved > 0)
        {
            // The options built from the settings are cached by the tenant.
            _shellReleaseManager.RequestRelease();

            await _notifier.SuccessAsync(H.Plural(moved, "1 credential was moved to a secret.", "{0} credentials were moved to secrets."));
        }

        if (moved < migration.Results.Count)
        {
            await _notifier.ErrorAsync(H["Some credentials could not be moved. Their settings were left unchanged."]);
            model.Editor = await _displayManager.BuildEditorAsync(new SecretMigration { Store = model.Store }, _updateModelAccessor.ModelUpdater, false);

            return View(nameof(Index), model);
        }

        return RedirectToAction(nameof(Index));
    }

    private List<string> GetWritableStores()
        => _secretManager.GetStores()
            .Where(store => !store.IsReadOnly)
            .Select(store => store.Name)
            .ToList();
}
