using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.CustomSettings.Services;
using OrchardCore.Mvc.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.CustomSettings
{
    public class AdminMenu : INavigationProvider
    {
        private readonly CustomSettingsService _customSettingsService;
        protected readonly IStringLocalizer S;

        public AdminMenu(
            IStringLocalizer<AdminMenu> localizer,
            CustomSettingsService customSettingsService)
        {
            S = localizer;
            _customSettingsService = customSettingsService;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            foreach (var type in _customSettingsService.GetAllSettingsTypes())
            {
                builder
                    .Add(S["Configuration"], configuration => configuration
                        .Add(S["Settings"], settings => settings
                            .Add(new LocalizedString(type.DisplayName, type.DisplayName), type.DisplayName.PrefixPosition(), layers => layers
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = type.Name })
                                .AddClass(type.Name.HtmlClassify())
                                .Id(type.Name.HtmlClassify())
                                .Permission(Permissions.CreatePermissionForType(type))
                                .Resource(type.Name)
                                .LocalNav()
                            )));
            }

            return Task.CompletedTask;
        }
    }
}
