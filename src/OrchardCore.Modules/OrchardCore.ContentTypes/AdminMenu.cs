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
        protected readonly IStringLocalizer S;

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
                .Add(S["Content Definition"], S["Content Definition"].PrefixPosition("9"), contentDefinition => contentDefinition
                    .Add(S["Content Types"], S["Content Types"].PrefixPosition("1"), contentTypes => contentTypes
                        .Action(nameof(AdminController.List), adminControllerName, new { area = "OrchardCore.ContentTypes" })
                        .Permission(Permissions.ViewContentTypes)
                        .LocalNav())
                    .Add(S["Content Parts"], S["Content Parts"].PrefixPosition("2"), contentParts => contentParts
                        .Action(nameof(AdminController.ListParts), adminControllerName, new { area = "OrchardCore.ContentTypes" })
                        .Permission(Permissions.ViewContentTypes)
                        .LocalNav())
                    ));

            return Task.CompletedTask;
        }
    }
}
