using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Features.Services;
using OrchardCore.Features.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Routing;

namespace OrchardCore.Tenants.Controllers;

[Feature("OrchardCore.Tenants.TenantFeatures")]
[Admin]
public class TenantFeaturesController : Controller
{
    private readonly IExtensionManager _extensionManager;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly IShellHost _shellHost;
    private readonly IAuthorizationService _authorizationService;
    private readonly ShellSettings _shellSettings;
    private readonly INotifier _notifier;
    private readonly IStringLocalizer S;
    private readonly IHtmlLocalizer H;

    public TenantFeaturesController(
        IExtensionManager extensionManager,
        IHtmlLocalizer<AdminController> localizer,
        IShellFeaturesManager shellFeaturesManager,
        IShellHost shellHost,
        IAuthorizationService authorizationService,
        ShellSettings shellSettings,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _extensionManager = extensionManager;
        _shellFeaturesManager = shellFeaturesManager;
        _shellHost = shellHost;
        _authorizationService = authorizationService;
        _shellSettings = shellSettings;
        _notifier = notifier;
        H = localizer;
        S = stringLocalizer;
    }

    public async Task<ActionResult> Features(string tenant)
    {
        if (String.IsNullOrWhiteSpace(tenant) || !_shellHost.TryGetSettings(tenant, out var settings))
        {
            return NotFound();
        }

        if (settings.IsDefaultShell() || !await _authorizationService.AuthorizeAsync(User, FeaturePermissions.ManageTenantFeatures))
        {
            return Forbid();
        }

        var moduleFeatures = await ExecuteAsync(settings, async (featureService) =>
        {
            return await featureService.GetModuleFeaturesAsync();
        });

        return View(new ViewModels.TenantFeaturesViewModel
        {
            Tenant = tenant,
            Features = moduleFeatures,
        });
    }

    [HttpPost]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> Features(string tenant, BulkActionViewModel model, bool? force)
    {
        if (String.IsNullOrWhiteSpace(tenant) || !_shellHost.TryGetSettings(tenant, out var settings))
        {
            return NotFound();
        }

        if (settings.IsDefaultShell() || !await _authorizationService.AuthorizeAsync(User, FeaturePermissions.ManageTenantFeatures))
        {
            return Forbid();
        }

        if (model.FeatureIds == null || !model.FeatureIds.Any())
        {
            ModelState.AddModelError(nameof(BulkActionViewModel.FeatureIds), S["Please select one or more features."]);
        }

        if (ModelState.IsValid)
        {
            await ExecuteAsync(settings, async (featureService) =>
            {
                var availableFeatures = await featureService.GetAvailableFeatures(model.FeatureIds);

                await featureService.EnableOrDisableFeaturesAsync(availableFeatures, model.BulkAction, force, async (features, isEnabled) => await NotifyAsync(features, isEnabled));
            });
        }

        return RedirectToAction(nameof(Features));
    }

    [HttpPost]
    public async Task<IActionResult> Disable(string tenant, string id)
    {
        if (String.IsNullOrWhiteSpace(tenant) || !_shellHost.TryGetSettings(tenant, out var settings))
        {
            return NotFound();
        }

        if (settings.IsDefaultShell() || !await _authorizationService.AuthorizeAsync(User, FeaturePermissions.ManageTenantFeatures))
        {
            return Forbid();
        }

        var success = await ExecuteAsync(settings, async (featureService) =>
        {
            var feature = await featureService.GetAvailableFeature(id);

            if (feature == null)
            {
                return false;
            }

            await featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Disable, true, async (features, isEnabled) => await NotifyAsync(features, isEnabled));

            return true;
        });

        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Features), new { tenant });
    }

    [HttpPost]
    public async Task<IActionResult> Enable(string tenant, string id)
    {
        if (String.IsNullOrWhiteSpace(tenant) || !_shellHost.TryGetSettings(tenant, out var settings))
        {
            return NotFound();
        }

        if (settings.IsDefaultShell() || !await _authorizationService.AuthorizeAsync(User, FeaturePermissions.ManageTenantFeatures))
        {
            return Forbid();
        }

        var success = await ExecuteAsync(settings, async (featureService) =>
        {
            var feature = await featureService.GetAvailableFeature(id);

            if (feature == null)
            {
                return false;
            }

            await featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Enable, true, async (features, isEnabled) => await NotifyAsync(features, isEnabled));

            return true;
        });

        if (!success)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Features), new { tenant });
    }

    private async Task ExecuteAsync(ShellSettings settings, Func<FeatureService, Task> action)
    {
        var shellScope = await _shellHost.GetScopeAsync(settings);

        await shellScope.UsingAsync(async scope =>
        {
            var shellFeatureManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var extensionManager = scope.ServiceProvider.GetRequiredService<IExtensionManager>();

            var featureService = new FeatureService(shellFeatureManager, extensionManager);

            await action(featureService);
        });
    }

    private async Task<T> ExecuteAsync<T>(ShellSettings settings, Func<FeatureService, Task<T>> action)
    {
        var shellScope = await _shellHost.GetScopeAsync(settings);
        T result = default;
        await shellScope.UsingAsync(async scope =>
        {
            var shellFeatureManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
            var extensionManager = scope.ServiceProvider.GetRequiredService<IExtensionManager>();

            var featureService = new FeatureService(shellFeatureManager, extensionManager);

            result = await action(featureService);
        });

        return result;
    }

    private async Task NotifyAsync(IEnumerable<IFeatureInfo> features, bool enabled)
    {
        foreach (var feature in features)
        {
            await _notifier.SuccessAsync(H["{0} was {1}.", feature.Name ?? feature.Id, enabled ? "enabled" : "disabled"]);
        }
    }
}
