using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Navigation;
using OrchardCore.Environment.Shell.Descriptor.Models;

namespace OrchardCore.OpenId
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
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(T["OpenID Connect"], "15", category =>
            {
                category.AddClass("openid").Id("openid");

                var features = _shellDescriptor.Features.Select(feature => feature.Id).ToImmutableArray();
                if (features.Contains(OpenIdConstants.Features.Client) ||
                    features.Contains(OpenIdConstants.Features.Server) ||
                    features.Contains(OpenIdConstants.Features.Validation))
                {
                    category.Add(T["Settings"], "1", settings =>
                    {
                        if (features.Contains(OpenIdConstants.Features.Client))
                        {
                            settings.Add(T["Authentication client"], "1", client => client
                                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "OrchardCore.OpenId.Client" })
                                    .Permission(Permissions.ManageClientSettings)
                                    .LocalNav());
                        }

                        if (features.Contains(OpenIdConstants.Features.Server))
                        {
                            settings.Add(T["Authorization server"], "2", server => server
                                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "OrchardCore.OpenId.Server" })
                                    .Permission(Permissions.ManageServerSettings)
                                    .LocalNav());
                        }

                        if (features.Contains(OpenIdConstants.Features.Validation))
                        {
                            settings.Add(T["Token validation"], "3", validation => validation
                                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "OrchardCore.OpenId.Validation" })
                                    .Permission(Permissions.ManageValidationSettings)
                                    .LocalNav());
                        }
                    });
                }

                if (features.Contains(OpenIdConstants.Features.Management))
                {
                    category.Add(T["Management"], "2", management =>
                    {
                        management.Add(T["Applications"], "1", applications => applications
                                  .Action("Index", "Application", "OrchardCore.OpenId")
                                  .Permission(Permissions.ManageApplications)
                                  .LocalNav());

                        management.Add(T["Scopes"], "2", applications => applications
                                  .Action("Index", "Scope", "OrchardCore.OpenId")
                                  .Permission(Permissions.ManageScopes)
                                  .LocalNav());
                    });
                }
            });

            return Task.CompletedTask;
        }
    }
}
