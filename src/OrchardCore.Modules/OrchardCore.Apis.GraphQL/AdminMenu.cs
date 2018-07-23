using System;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;

namespace OrchardCore.Apis.GraphQL
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }
        
        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder
                .Add(S["Configuration"], "10", design => design
                    .AddClass("menu-configuration").Id("configuration")
                    .Add(S["GraphQL Explorer"], "5", deployment => deployment
                        .Action("Index", "Admin", new { area = "OrchardCore.Apis.GraphQL" })
                        .Permission(Permissions.ExecuteGraphQL)
                        .LocalNav()
                    )
                );
        }
    }
}
