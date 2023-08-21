using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Media.Azure
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

            builder.Add(S["Configuration"], configuration => configuration
                .Add(S["Media"], S["Media"].PrefixPosition(), media => media
                    .Add(S["Azure Blob Options"], S["Azure Blob Options"].PrefixPosition(), options => options
                        .Action("Options", "Admin", new { area = "OrchardCore.Media.Azure" })
                        .Permission(Permissions.ViewAzureMediaOptions)
                        .LocalNav())
            ));

            return Task.CompletedTask;
        }
    }
}
