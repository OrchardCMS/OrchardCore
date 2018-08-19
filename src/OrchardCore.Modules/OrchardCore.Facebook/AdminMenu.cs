using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Navigation;
using OrchardCore.Environment.Shell.Descriptor.Models;

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

        public void BuildNavigation(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            builder.Add(T["Facebook"], "15", category =>
            {
                category.AddClass("facebook").Id("facebook");

                var features = _shellDescriptor.Features.Select(feature => feature.Id).ToImmutableArray();
                if (features.Contains(FacebookConstants.Features.Core))
                {
                    category.Add(T["Settings"], "1", settings =>
                    {
                        if (features.Contains(FacebookConstants.Features.Core))
                        {
                            settings.Add(T["Facebook application"], "1", client => client
                                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = FacebookConstants.Features.Core })
                                    .Permission(Permissions.ManageFacebookApp)
                                    .LocalNav());
                        }
                    });
                }
            });
        }
    }
}
