using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Facebook
{
    public class AdminMenu : INavigationProvider
    {
        private readonly ShellDescriptor _shellDescriptor;

        public AdminMenu(
            IStringLocalizer<AdminMenu> localizer,
            ShellDescriptor shellDescriptor)
        {
            T = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(T["Configuration"], "15", category =>
                    category.Add(T["Facebook"], "1", settings =>
                        settings.AddClass("facebook").Id("facebook")
                                .Add(T["Application"], "1", client => client
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Core })
                                .Permission(Permissions.ManageFacebookApp)
                                .LocalNav())
                        ));
            }
            return Task.CompletedTask;
        }
    }

    [Feature(FacebookConstants.Features.Login)]
    public class AdminMenuLogin : INavigationProvider
    {
        private readonly ShellDescriptor _shellDescriptor;

        public AdminMenuLogin(
            IStringLocalizer<AdminMenuLogin> localizer,
            ShellDescriptor shellDescriptor)
        {
            T = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public IStringLocalizer T { get; set; }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(T["Configuration"], "15", category =>
                    category.Add(T["Facebook"], "1", settings =>
                        settings.AddClass("facebook").Id("facebook")
                                .Add(T["Login"], "2", client => client
                                .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Login })
                                .Permission(Permissions.ManageFacebookApp)
                                .LocalNav())
                    ));
            }

            return Task.CompletedTask;
        }
    }
}
