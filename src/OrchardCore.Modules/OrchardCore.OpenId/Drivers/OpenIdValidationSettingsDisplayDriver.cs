using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;

namespace OrchardCore.OpenId.Drivers
{
    public class OpenIdValidationSettingsDisplayDriver : DisplayDriver<OpenIdValidationSettings>
    {
        private readonly IShellHost _shellHost;

        public OpenIdValidationSettingsDisplayDriver(IShellHost shellHost)
            => _shellHost = shellHost;

        public override Task<IDisplayResult> EditAsync(OpenIdValidationSettings settings, BuildEditorContext context)
            => Task.FromResult<IDisplayResult>(Initialize<OpenIdValidationSettingsViewModel>("OpenIdValidationSettings_Edit", async model =>
            {
                model.Authority = settings.Authority;
                model.Audience = settings.Audience;
                model.Tenant = settings.Tenant;

                var availableTenants = new List<string>();

                foreach (var shellContext in (await _shellHost.ListShellContextsAsync())
                    .Where(s => s.Settings.State == TenantState.Running))
                {
                    using (var scope = shellContext.CreateScope())
                    {
                        var descriptor = scope.ServiceProvider.GetRequiredService<ShellDescriptor>();
                        if (descriptor.Features.Any(feature => feature.Id == OpenIdConstants.Features.Server))
                        {
                            availableTenants.Add(shellContext.Settings.Name);
                        }
                    }
                }

                model.AvailableTenants = availableTenants;

            }).Location("Content:2"));

        public override async Task<IDisplayResult> UpdateAsync(OpenIdValidationSettings settings, UpdateEditorContext context)
        {
            var model = new OpenIdValidationSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            settings.Authority = model.Authority?.Trim();
            settings.Audience = model.Audience?.Trim();
            settings.Tenant = model.Tenant;

            return await EditAsync(settings, context);
        }
    }
}
