using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.Controllers;
using OrchardCore.Navigation;
using System;
using System.Threading.Tasks;

namespace OrchardCore.ContentTypes {
    public class AdminMenu : INavigationProvider
    {
        private readonly IStringLocalizer S;

        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Content"], content => content
                .Add(S["Content Definition"], "2", contentDefinition => contentDefinition
                    .Add(S["Content Types"], "1", contentTypes => contentTypes
                        .Action(nameof(AdminController.List), typeof(AdminController).ControllerName(), new { area = "OrchardCore.ContentTypes" })
                        .Permission(Permissions.ViewContentTypes)
                        .LocalNav())
                    .Add(S["Content Parts"], "2", contentParts => contentParts
                        .Action(nameof(AdminController.ListParts), typeof(AdminController).ControllerName(), new { area = "OrchardCore.ContentTypes" })
                        .Permission(Permissions.ViewContentTypes)
                        .LocalNav())
                    ));

            return Task.CompletedTask;
        }
    }
}
