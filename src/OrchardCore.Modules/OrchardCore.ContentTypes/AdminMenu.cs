using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentTypes.Controllers;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;

namespace OrchardCore.ContentTypes
{
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

            var adminControllerName = typeof(AdminController).ControllerName();

            builder.Add(S["Content"], content => content
                .Add(S["Content Definition"], S["am-Content Definition"], contentDefinition => contentDefinition
                    .Add(S["Content Types"], S["am1-Content Types"], contentTypes => contentTypes
                        .Action(nameof(AdminController.List), adminControllerName, new { area = "OrchardCore.ContentTypes" })
                        .Permission(Permissions.ViewContentTypes)
                        .LocalNav())
                    .Add(S["Content Parts"], S["am2-Content Parts"], contentParts => contentParts
                        .Action(nameof(AdminController.ListParts), adminControllerName, new { area = "OrchardCore.ContentTypes" })
                        .Permission(Permissions.ViewContentTypes)
                        .LocalNav())
                    ));

            return Task.CompletedTask;
        }
    }
}
