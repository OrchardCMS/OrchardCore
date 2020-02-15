using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Modules;
using OrchardCore.Navigation;

namespace OrchardCore.Twitter
{
    [Feature(TwitterConstants.Features.Twitter)]
    public class AdminMenu : INavigationProvider
    {
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IStringLocalizer S;

        public AdminMenu(
            IStringLocalizer<AdminMenu> localizer,
            ShellDescriptor shellDescriptor)
        {
            S = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Authentication"], authentication => authentication
                        .Add(S["Twitter"], "18", settings => settings
                        .AddClass("twitter").Id("twitter")
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = TwitterConstants.Features.Twitter })
                            .Permission(Permissions.ManageTwitter)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }

    [Feature(TwitterConstants.Features.Signin)]
    public class AdminMenuSignin: INavigationProvider
    {
        private readonly ShellDescriptor _shellDescriptor;
        private readonly IStringLocalizer S;

        public AdminMenuSignin(
            IStringLocalizer<AdminMenuSignin> localizer,
            ShellDescriptor shellDescriptor)
        {
            S = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                builder.Add(S["Security"], security => security
                        .Add(S["Twitter"], "15", settings => settings
                        .AddClass("twitter").Id("twitter")                        
                        .Add(S["Sign in with Twitter"], "15", client => client
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = TwitterConstants.Features.Signin })
                            .Permission(Permissions.ManageTwitterSignin)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }

}
