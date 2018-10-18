using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.OpenId.Services;
using OrchardCore.OpenId.Settings;
using OrchardCore.OpenId.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.OpenId.Drivers
{
    public class OpenIdValidationSettingsDisplayDriver : SectionDisplayDriver<ISite, OpenIdValidationSettings>
    {
        private const string SettingsGroupId = "OrchardCore.OpenId.Validation";

        private readonly IAuthorizationService _authorizationService;
        private readonly IOpenIdValidationService _validationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotifier _notifier;
        private readonly IHtmlLocalizer<OpenIdValidationSettingsDisplayDriver> T;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly IShellSettingsManager _shellSettingsManager;

        public OpenIdValidationSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IOpenIdValidationService validationService,
            IHttpContextAccessor httpContextAccessor,
            INotifier notifier,
            IHtmlLocalizer<OpenIdValidationSettingsDisplayDriver> stringLocalizer,
            IShellHost shellHost,
            ShellSettings shellSettings,
            IShellSettingsManager shellSettingsManager)
        {
            _authorizationService = authorizationService;
            _validationService = validationService;
            _notifier = notifier;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _shellSettingsManager = shellSettingsManager;
            T = stringLocalizer;
        }

        public override async Task<IDisplayResult> EditAsync(OpenIdValidationSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageValidationSettings))
            {
                return null;
            }

            return Initialize<OpenIdValidationSettingsViewModel>("OpenIdValidationSettings_Edit", async model =>
            {
                model.Authority = settings.Authority;
                model.Audience = settings.Audience;
                model.Tenant = settings.Tenant;

                var availableTenants = new List<string>();
                var tenants = _shellSettingsManager.LoadSettings().Where(s => s.State == TenantState.Running);

                foreach(var shellContext in await _shellHost.ListShellContextsAsync())
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

            }).Location("Content:2").OnGroup(SettingsGroupId);
        }

        public override async Task<IDisplayResult> UpdateAsync(OpenIdValidationSettings settings, BuildEditorContext context)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !await _authorizationService.AuthorizeAsync(user, Permissions.ManageValidationSettings))
            {
                return null;
            }

            if (context.GroupId == SettingsGroupId)
            {
                var model = new OpenIdValidationSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                settings.Authority = model.Authority?.Trim();
                settings.Audience = model.Audience?.Trim();
                settings.Tenant = model.Tenant;

                foreach (var result in await _validationService.ValidateSettingsAsync(settings))
                {
                    if (result != ValidationResult.Success)
                    {
                        var key = result.MemberNames.FirstOrDefault() ?? string.Empty;
                        context.Updater.ModelState.AddModelError(key, result.ErrorMessage);
                    }
                }

                // If the settings are valid, reload the current tenant.
                if (context.Updater.ModelState.IsValid)
                {
                    await _shellHost.ReloadShellContextAsync(_shellSettings);
                }
            }

            return await EditAsync(settings, context);
        }
    }
}
