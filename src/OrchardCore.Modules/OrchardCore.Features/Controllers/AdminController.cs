using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Environment.Extensions;
using OrchardCore.Environment.Extensions.Features;
using OrchardCore.Environment.Shell;
using OrchardCore.Features.Models;
using OrchardCore.Features.Services;
using OrchardCore.Features.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Routing;
using static OrchardCore.Localization.LocalizationMetadata;

namespace OrchardCore.Features.Controllers;

public sealed class AdminController : Controller
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IExtensionManager _extensionManager;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly AdminOptions _adminOptions;
    private readonly INotifier _notifier;

    internal readonly IStringLocalizer S;
    internal readonly IHtmlLocalizer H;

    public AdminController(
        IAuthorizationService authorizationService,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IExtensionManager extensionManager,
        IShellFeaturesManager shellFeaturesManager,
        IOptions<AdminOptions> adminOptions,
        INotifier notifier,
        IStringLocalizer<AdminController> stringLocalizer,
        IHtmlLocalizer<AdminController> htmlLocalizer)
    {
        _authorizationService = authorizationService;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _extensionManager = extensionManager;
        _shellFeaturesManager = shellFeaturesManager;
        _adminOptions = adminOptions.Value;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    [Admin("Features/{tenant?}", "Features")]
    public async Task<ActionResult> Features(
        string tenant,
        [FromServices] IShapeFactory shapeFactory)
    {
        if (!await _authorizationService.AuthorizeAsync(User, FeaturesPermissions.ManageFeatures))
        {
            return Forbid();
        }

        var viewModel = new FeaturesViewModel();

        await ExecuteAsync(tenant, async (featureService, settings, isProxy) =>
        {
            // If the user provide an invalid tenant value, we'll set it to null so it's not available on the next request.
            if (isProxy)
            {
                viewModel.IsProxy = true;
                tenant = settings.Name;
                viewModel.Name = settings.Name;
            }

            viewModel.Features = await featureService.GetModuleFeaturesAsync();
        });

        // The categories share one layout, so the page offers it once, beside its filters, and its lists do not
        // carry a selector of their own.
        viewModel.LayoutSelector = await shapeFactory.CreateAsync(AdminListConstants.LayoutSelectorShapeType, Arguments.From(new
        {
            ListName = FeaturesAdminList.Name,
        }));

        // The page keeps one list per category, and every list follows the configured layout.
        foreach (var group in viewModel.Features.GroupBy(feature => feature.Descriptor.Category).OrderBy(group => group.Key))
        {
            var category = group.Key ?? S["Uncategorized"].Value;
            var entries = group.OrderBy(feature => feature.Descriptor.Name)
                .Select(feature => CreateEntry(viewModel, feature, category, tenant)).ToArray();
            var hasSelectableFeature = entries.Any(entry => entry.IsSelectable);

            // A category with nothing to select has no select-all checkbox, so it gets no toolbar at all, not even
            // the generic one.
            var toolbar = hasSelectableFeature
                ? await shapeFactory.CreateAsync("FeaturesGroupToolbar", Arguments.From(new
                {
                    CategoryName = category,
                    CheckboxId = $"select-all-{category.HtmlClassify()}",
                }))
                : null;

            viewModel.Groups.Add(new FeatureGroupViewModel
            {
                Category = category,
                List = new AdminListContext<FeatureEntry>
                {
                    Source = entries,
                    Toolbar = toolbar,
                    ShowLayoutSelector = false,
                },
            });
        }

        return View(viewModel);
    }

    internal static FeatureEntry CreateEntry(FeaturesViewModel viewModel, ModuleFeature feature, string category, string tenant)
    {
        var descriptor = feature.Descriptor;
        var isAlwaysEnabled = feature.IsAlwaysEnabled;

        if (viewModel.IsProxy && descriptor.Id == FeaturesConstants.FeatureId)
        {
            isAlwaysEnabled = false;
        }

        // The features this one depends on, whether it declares them itself or reaches them through a dependency.
        var dependencies = feature.FeatureDependencies
            .Select(dependency => viewModel.Features.FirstOrDefault(f => f.Descriptor.Id == dependency.Id))
            .Where(f => f != null)
            .OrderBy(f => f.Descriptor.Name)
            .Select(f => f.Descriptor)
            .ToList();

        var missingDependencies = feature.FeatureDependencies
            .Where(dependency => !viewModel.Features.Any(f => f.Descriptor.Id == dependency.Id))
            .ToList();

        return new FeatureEntry
        {
            Feature = feature,
            Tenant = tenant,
            IsAlwaysEnabled = isAlwaysEnabled,
            CanDisable = !descriptor.EnabledByDependencyOnly && !isAlwaysEnabled && category != "Core" && descriptor.Name != Application.ModuleName,
            CanEnable = !descriptor.EnabledByDependencyOnly && missingDependencies.Count == 0 && descriptor.Id != "OrchardCore.Setup" && descriptor.Id != "OrchardCore.AutoSetup",
            DirectDependencies = dependencies.Where(dependency => descriptor.Dependencies.Contains(dependency.Id)).ToList(),
            IndirectDependencies = dependencies.Where(dependency => !descriptor.Dependencies.Contains(dependency.Id)).ToList(),
            MissingDependencies = missingDependencies.ToList(),
        };
    }

    [HttpPost]
    [FormValueRequired("submit.BulkAction")]
    public async Task<ActionResult> Features(BulkActionViewModel model, bool? force, string tenant)
    {
        if (!await _authorizationService.AuthorizeAsync(User, FeaturesPermissions.ManageFeatures))
        {
            return Forbid();
        }

        if (model.FeatureIds == null || model.FeatureIds.Length == 0)
        {
            ModelState.AddModelError(nameof(BulkActionViewModel.FeatureIds), S["Please select one or more features."]);
        }

        if (ModelState.IsValid)
        {
            await ExecuteAsync(tenant, async (featureService, settings, isProxy) =>
            {
                var availableFeatures = await featureService.GetAvailableFeatures(model.FeatureIds);

                await featureService.EnableOrDisableFeaturesAsync(availableFeatures, model.BulkAction, force, async (features, isEnabled) => await NotifyAsync(features.ToArray(), isEnabled));
            });
        }

        return RedirectToAction(nameof(Features));
    }

    [HttpPost]
    [Admin("Features/{id}/Disable/{tenant?}", "FeaturesDisable")]
    public async Task<IActionResult> Disable(string id, string tenant)
    {
        if (!await _authorizationService.AuthorizeAsync(User, FeaturesPermissions.ManageFeatures))
        {
            return Forbid();
        }

        var found = false;

        await ExecuteAsync(tenant, async (featureService, settings, isProxy) =>
        {
            var feature = await featureService.GetAvailableFeature(id);

            if (feature == null)
            {
                return;
            }

            found = true;

            if (!isProxy && id == FeaturesConstants.FeatureId)
            {
                await _notifier.ErrorAsync(H["This feature is always enabled and cannot be disabled."]);

                return;
            }

            await featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Disable, true, async (features, isEnabled) => await NotifyAsync(features.ToArray(), isEnabled));
        });

        if (!found)
        {
            return NotFound();
        }

        return Redirect(GetNextUrl(tenant, id));
    }

    [HttpPost]
    [Admin("Features/{id}/Enable/{tenant?}", "FeaturesEnable")]
    public async Task<IActionResult> Enable(string id, string tenant)
    {
        if (!await _authorizationService.AuthorizeAsync(User, FeaturesPermissions.ManageFeatures))
        {
            return Forbid();
        }

        var found = false;

        await ExecuteAsync(tenant, async (featureService, settings, isProxy) =>
        {
            var feature = await featureService.GetAvailableFeature(id);

            if (feature == null)
            {
                return;
            }

            found = true;

            await featureService.EnableOrDisableFeaturesAsync(new[] { feature }, FeaturesBulkAction.Enable, true, async (features, isEnabled) => await NotifyAsync(features.ToArray(), isEnabled));
        });

        if (!found)
        {
            return NotFound();
        }

        return Redirect(GetNextUrl(tenant, id));
    }

    private async Task ExecuteAsync(string tenant, Func<FeatureService, ShellSettings, bool, Task> action)
    {
        if (_shellSettings.IsDefaultShell() &&
            !string.IsNullOrWhiteSpace(tenant) &&
            _shellHost.TryGetSettings(tenant, out var settings) &&
            !settings.IsDefaultShell() &&
            settings.IsRunning())
        {
            // At this point, we know that this request is being executed from the Default tenant.
            // Also, we were able to find a matching and running tenant that isn't the Default one.
            // Therefore, it is safe to create a scope for the given tenant.
            var shellScope = await _shellHost.GetScopeAsync(settings);

            await shellScope.UsingAsync(async scope =>
            {
                var shellFeatureManager = scope.ServiceProvider.GetRequiredService<IShellFeaturesManager>();
                var extensionManager = scope.ServiceProvider.GetRequiredService<IExtensionManager>();

                // At this point we apply the action on the given tenant.
                await action(new FeatureService(shellFeatureManager, extensionManager), scope.ShellContext.Settings, true);
            });

            return;
        }

        // At this point we apply the action on the current tenant.
        await action(new FeatureService(_shellFeaturesManager, _extensionManager), _shellSettings, false);
    }

    private string GetNextUrl(string tenant, string featureId)
    {
        // Generating routes can fail while the tenant is recycled as routes can use services.
        // It could be fixed by waiting for the next request or the end of the current one
        // to actually release the tenant. Right now we render the url before recycling the tenant.

        ShellSettings settings = null;

        if (!string.IsNullOrWhiteSpace(tenant))
        {
            _shellHost.TryGetSettings(tenant, out settings);
        }

        if (settings != null && settings.Name != _shellSettings.Name)
        {
            return Url.Action(nameof(Features), new { tenant });
        }

        if (!settings.IsDefaultShell() && featureId == FeaturesConstants.FeatureId)
        {
            return Url.Content("~/" + _adminOptions.AdminUrlPrefix);
        }

        return Url.Action(nameof(Features));
    }

    private async ValueTask NotifyAsync(IFeatureInfo[] features, bool enabled = true)
    {
        if (enabled)
        {
            await _notifier.SuccessAsync(H.Plural(Arg("number of features", features.Length), "The feature {1} was enabled.", "The following features were enabled: {1}.", Arg("feature names", string.Join(", ", features.Select(f => f.Name ?? f.Id)))));
        }
        else
        {
            await _notifier.SuccessAsync(H.Plural(Arg("number of features", features.Length), "The feature {1} was disabled.", "The following features were disabled: {1}.", Arg("feature names", string.Join(", ", features.Select(f => f.Name ?? f.Id)))));
        }
    }
}
