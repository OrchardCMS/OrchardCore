using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
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
                model.Authority = settings.Authority?.AbsoluteUri;
                model.MetadataAddress = settings.MetadataAddress?.AbsoluteUri;
                model.Audience = settings.Audience;
                model.DisableTokenTypeValidation = settings.DisableTokenTypeValidation;
                model.Tenant = settings.Tenant;

                var availableTenants = new List<string>();

                foreach (var shellSettings in _shellHost.GetAllSettings().Where(s => s.IsRunning()))
                {
                    var shellScope = await _shellHost.GetScopeAsync(shellSettings);

                    await shellScope.UsingAsync(scope =>
                    {
                        var descriptor = scope.ServiceProvider.GetRequiredService<ShellDescriptor>();
                        if (descriptor.Features.Any(feature => feature.Id == OpenIdConstants.Features.Server))
                        {
                            availableTenants.Add(shellSettings.Name);
                        }
                        return Task.CompletedTask;
                    });
                }

                model.AvailableTenants = availableTenants;
            }).Location("Content:2"));

        public override async Task<IDisplayResult> UpdateAsync(OpenIdValidationSettings settings, UpdateEditorContext context)
        {
            var model = new OpenIdValidationSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            settings.Authority = !string.IsNullOrEmpty(model.Authority) ? new Uri(model.Authority, UriKind.Absolute) : null;
            settings.MetadataAddress = !string.IsNullOrEmpty(model.MetadataAddress) ? new Uri(model.MetadataAddress, UriKind.Absolute) : null;
            settings.Audience = model.Audience?.Trim();
            settings.DisableTokenTypeValidation = model.DisableTokenTypeValidation;
            settings.Tenant = model.Tenant;

            return await EditAsync(settings, context);
        }
    }
}
