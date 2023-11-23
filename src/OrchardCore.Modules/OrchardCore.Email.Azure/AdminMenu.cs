using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;

namespace OrchardCore.Email.Azure
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
            if (!string.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
                return Task.CompletedTask;

            builder
                .Add(S["Configuration"], configuration => configuration
                    .Add(S["Settings"], settings => settings
                        .Add(S["Email"], S["Email"].PrefixPosition(), email => email
                            .AddClass("email").Id("email")
                            .Add(S["Azure Email Settings"], S["Azure Email Settings"].PrefixPosition(), options => options
                                .Action("Options", "Admin", new { area = "OrchardCore.Email.Azure" })
                                .Permission(Permissions.ViewAzureEmailOptions)
                                .LocalNav())
                )));

            return Task.CompletedTask;
        }
    }
}
