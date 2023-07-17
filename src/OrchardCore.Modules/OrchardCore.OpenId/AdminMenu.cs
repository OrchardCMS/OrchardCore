using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Environment.Shell.Descriptor.Models;
using OrchardCore.Navigation;

namespace OrchardCore.OpenId
{
    public class AdminMenu : INavigationProvider
    {
        private readonly ShellDescriptor _shellDescriptor;
        protected readonly IStringLocalizer S;

        public AdminMenu(
            IStringLocalizer<AdminMenu> localizer,
            ShellDescriptor shellDescriptor)
        {
            S = localizer;
            _shellDescriptor = shellDescriptor;
        }

        public Task BuildNavigationAsync(string name, NavigationBuilder builder)
        {
            if (!String.Equals(name, "admin", StringComparison.OrdinalIgnoreCase))
            {
                return Task.CompletedTask;
            }

            builder.Add(S["Security"], security => security
            .Add(S["OpenID Connect"], S["OpenID Connect"].PrefixPosition(), category =>
            {
                category.AddClass("openid").Id("openid");

                var features = _shellDescriptor.Features.Select(feature => feature.Id).ToImmutableArray();
                if (features.Contains(OpenIdConstants.Features.Client) ||
                    features.Contains(OpenIdConstants.Features.Server) ||
                    features.Contains(OpenIdConstants.Features.Validation))
                {
                    category.Add(S["Settings"], "1", settings =>
                    {
                        if (features.Contains(OpenIdConstants.Features.Client))
                        {
                            settings.Add(S["Authentication client"], "1", client => client
                                    .Action("Index", "Admin", new { area = "OrchardCore.Settings", groupId = "OrchardCore.OpenId.Client" })
                                    .Permission(Permissions.ManageClientSettings)
                                    .LocalNav());
                        }

                        if (features.Contains(OpenIdConstants.Features.Server))
                        {
                            settings.Add(S["Authorization server"], "2", server => server
                                    .Action("Index", "ServerConfiguration", "OrchardCore.OpenId")
                                    .Permission(Permissions.ManageServerSettings)
                                    .LocalNav());
                        }

                        if (features.Contains(OpenIdConstants.Features.Validation))
                        {
                            settings.Add(S["Token validation"], "3", validation => validation
                                    .Action("Index", "ValidationConfiguration", "OrchardCore.OpenId")
                                    .Permission(Permissions.ManageValidationSettings)
                                    .LocalNav());
                        }
                    });
                }

                if (features.Contains(OpenIdConstants.Features.Management))
                {
                    category.Add(S["Management"], "2", management =>
                    {
                        management.Add(S["Applications"], "1", applications => applications
                                  .Action("Index", "Application", "OrchardCore.OpenId")
                                  .Permission(Permissions.ManageApplications)
                                  .LocalNav());

                        management.Add(S["Scopes"], "2", applications => applications
                                  .Action("Index", "Scope", "OrchardCore.OpenId")
                                  .Permission(Permissions.ManageScopes)
                                  .LocalNav());
                    });
                }
            }));

            return Task.CompletedTask;
        }
    }
}
