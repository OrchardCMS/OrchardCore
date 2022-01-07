using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Data;
using OrchardCore.Environment.Shell;
using OrchardCore.Tenants.ViewModels;

namespace OrchardCore.Tenants.Controllers
{
    internal static class ControllerExtensions
    {
        internal static async Task ValidateModelAsync<TViewModel>(this Controller controller, TViewModel model, bool newTenant = true) where TViewModel : TenantViewModel
        {
            var shellHost = controller.HttpContext.RequestServices.GetService<IShellHost>();
            var featureProfilesService = controller.HttpContext.RequestServices.GetService<IFeatureProfilesService>();
            var databaseProviders = controller.HttpContext.RequestServices.GetServices<DatabaseProvider>();
            var S = controller.HttpContext.RequestServices.GetService<IStringLocalizer>();

            var selectedProvider = databaseProviders.FirstOrDefault(x => x.Value == model.DatabaseProvider);

            if (selectedProvider != null && selectedProvider.HasConnectionString && String.IsNullOrWhiteSpace(model.ConnectionString))
            {
                controller.ModelState.AddModelError(nameof(EditTenantViewModel.ConnectionString), S["The connection string is mandatory for this provider."]);
            }

            if (String.IsNullOrWhiteSpace(model.Name))
            {
                controller.ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["The tenant name is mandatory."]);
            }

            if (!String.IsNullOrWhiteSpace(model.FeatureProfile))
            {
                var featureProfiles = await featureProfilesService.GetFeatureProfilesAsync();
                if (!featureProfiles.ContainsKey(model.FeatureProfile))
                {
                    controller.ModelState.AddModelError(nameof(EditTenantViewModel.FeatureProfile), S["The feature profile does not exist.", model.FeatureProfile]);
                }
            }

            var allSettings = shellHost.GetAllSettings();

            if (newTenant && allSettings.Any(tenant => String.Equals(tenant.Name, model.Name, StringComparison.OrdinalIgnoreCase)))
            {
                controller.ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["A tenant with the same name already exists.", model.Name]);
            }

            if (!String.IsNullOrEmpty(model.Name) && !Regex.IsMatch(model.Name, @"^\w+$"))
            {
                controller.ModelState.AddModelError(nameof(EditTenantViewModel.Name), S["Invalid tenant name. Must contain characters only and no spaces."]);
            }

            if (!IsDefaultShell(controller) && String.IsNullOrWhiteSpace(model.RequestUrlHost) && String.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                controller.ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["Host and url prefix can not be empty at the same time."]);
            }

            var allOtherShells = allSettings.Where(t => !String.Equals(t.Name, model.Name, StringComparison.OrdinalIgnoreCase));
            if (allOtherShells.Any(t => String.Equals(t.RequestUrlPrefix, model.RequestUrlPrefix?.Trim(), StringComparison.OrdinalIgnoreCase) && (t.RequestUrlHost?.Contains(model.RequestUrlHost, StringComparison.OrdinalIgnoreCase) ?? false)))
            {
                controller.ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["A tenant with the same host and prefix already exists.", model.Name]);
            }

            if (!String.IsNullOrWhiteSpace(model.RequestUrlPrefix))
            {
                if (model.RequestUrlPrefix.Contains('/'))
                {
                    controller.ModelState.AddModelError(nameof(EditTenantViewModel.RequestUrlPrefix), S["The url prefix can not contain more than one segment."]);
                }
            }
        }

        internal static bool IsDefaultShell(this Controller controller)
        {
            var currentShellSettings = controller.HttpContext.RequestServices.GetService<ShellSettings>();

            return String.Equals(currentShellSettings.Name, ShellHelper.DefaultShellName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
