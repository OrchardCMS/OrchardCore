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
                builder.Add(T["Security"], security => security
                        .Add(T["Authentication"], authentication => authentication
                        .Add(T["Twitter"], "18", settings => settings
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

        public AdminMenuSignin(
            IStringLocalizer<AdminMenuSignin> localizer,
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
                builder.Add(T["Security"], security => security
                        .Add(T["Twitter"], "15", settings => settings
                        .AddClass("twitter").Id("twitter")                        
                        .Add(T["Sign in with Twitter"], "15", client => client
                            .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = TwitterConstants.Features.Signin })
                            .Permission(Permissions.ManageTwitterSignin)
                            .LocalNav())
                    ));
            }
            return Task.CompletedTask;
        }
    }

}
