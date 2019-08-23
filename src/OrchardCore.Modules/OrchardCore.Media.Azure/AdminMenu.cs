using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media.Azure
{
    public class AdminMenu : INavigationProvider
    {
        public AdminMenu(IStringLocalizer<AdminMenu> localizer)
        {
            S = localizer;
        }

        public IStringLocalizer S { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Configuration"], content => content
                .Add(S["Azure Asset Cache"], "1", contentItems => contentItems
                    .Action("Index", "Admin", new { area = "OrchardCore.Media.Azure" })
                    .Permission(Permissions.ManageAzureAssetCache)
                    .LocalNav())
                );

            return Task.CompletedTask;
        }
    }
}
