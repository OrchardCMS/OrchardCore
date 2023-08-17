using OrchardCore.OpenId.Abstractions.Descriptors;
using static OpenIddict.Abstractions.OpenIddictConstants.Permissions;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId
{
    internal class OpenIdApplicationStepTestsData
        : TheoryData<string, OpenIdApplicationDescriptor>
    {
        public OpenIdApplicationStepTestsData()
        {
            AddWithHashsets(
                "app-recipe1",
                new OpenIdApplicationDescriptor
                {
                    ClientId = "a1",
                    ClientSecret = "test-secret",
                    ConsentType = "explicit",
                    DisplayName = "Test Application",
                    Type = "confidential",
                },
                new[] { new Uri("https://localhost:111/logout-redirect"), new Uri("https://localhost:222/logout-redirect") },
                new[] { new Uri("https://localhost:111/redirect"), new Uri("https://localhost:222/redirect") },
                null,
                new[] {
                    GrantTypes.AuthorizationCode,
                    GrantTypes.RefreshToken,
                    Endpoints.Authorization,
                    Endpoints.Logout,
                    Endpoints.Token,
                    ResponseTypes.Code,
                });

            AddWithHashsets(
                "app-recipe2",
                new OpenIdApplicationDescriptor
                {
                    ClientId = "a2",
                    ClientSecret = "test-secret",
                    ConsentType = "explicit",
                    DisplayName = "Test Application",
                    Type = "confidential",
                },
                new[] { new Uri("https://localhost/logout-redirect") },
                new[] { new Uri("https://localhost/redirect") },
                new[] { "role1", "role2" },
                new[] {
                    GrantTypes.ClientCredentials,
                    Endpoints.Token,
                    $"{Prefixes.Scope}scope1"
                });
        }

        private static void UnionIfNotNull<TItem>(ISet<TItem> itemSet, IEnumerable<TItem> items)
        {
            if (items != null)
            {
                itemSet.UnionWith(items);
            }
        }

        private void AddWithHashsets(
            string recipeFile,
            OpenIdApplicationDescriptor app,
            IEnumerable<Uri> postLogoutRedirectUris,
            IEnumerable<Uri> redirectUris,
            IEnumerable<string> roles,
            IEnumerable<string> permissions)
        {
            UnionIfNotNull(app.PostLogoutRedirectUris, postLogoutRedirectUris);
            UnionIfNotNull(app.RedirectUris, redirectUris);
            UnionIfNotNull(app.Roles, roles);
            UnionIfNotNull(app.Permissions, permissions);
            Add(recipeFile, app);
        }
    }
}
