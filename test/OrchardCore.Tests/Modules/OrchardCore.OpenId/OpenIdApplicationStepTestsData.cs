using OrchardCore.OpenId.Abstractions.Descriptors;

namespace OrchardCore.Tests.Modules.OrchardCore.OpenId;

internal sealed class OpenIdApplicationStepTestsData
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
                ClientType = "confidential",
                ConsentType = "explicit",
                DisplayName = "Test Application"
            },
            new[] { new Uri("https://localhost:111/logout-redirect"), new Uri("https://localhost:222/logout-redirect") },
            new[] { new Uri("https://localhost:111/redirect"), new Uri("https://localhost:222/redirect") },
            null,
            new[] {
                OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                OpenIddictConstants.Permissions.Endpoints.Authorization,
                OpenIddictConstants.Permissions.Endpoints.Logout,
                OpenIddictConstants.Permissions.Endpoints.Token,
                OpenIddictConstants.Permissions.ResponseTypes.Code,
            });

        AddWithHashsets(
            "app-recipe2",
            new OpenIdApplicationDescriptor
            {
                ClientId = "a2",
                ClientSecret = "test-secret",
                ClientType = "confidential",
                ConsentType = "explicit",
                DisplayName = "Test Application"
            },
            new[] { new Uri("https://localhost/logout-redirect") },
            new[] { new Uri("https://localhost/redirect") },
            new[] { "role1", "role2" },
            new[] {
                OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                OpenIddictConstants.Permissions.Endpoints.Token,
                $"{OpenIddictConstants.Permissions.Prefixes.Scope}scope1"
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
