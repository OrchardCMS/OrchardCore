using OrchardCore.Admin;
using OrchardCore.Contents;
using OrchardCore.Roles;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Localization;

namespace OrchardCore.Tests.Modules.OrchardCore.Roles;

/// <summary>
/// Renders the roles editor of two tenants in several cultures, and checks that the description of a static
/// permission uses the PO file translation of the tenant, in the culture of each request.
/// </summary>
public class PermissionDescriptionLocalizationTests
{
    private const string Description = "Edit content for others";

    private static readonly string s_permissionContext = typeof(CommonPermissions).FullName;

    [Fact]
    public async Task StaticPermissionDescription_TwoTenantsAndCultures_UsesTheTranslationOfEachTenantAndCulture()
    {
        // Arrange. CommonPermissions.EditContent is a static object that every tenant of the process shares.
        using var tenantA = await CreateTenantAsync(
            ("fr", "Modifier le contenu des autres A"),
            ("de", "Inhalte anderer bearbeiten A"));

        using var tenantB = await CreateTenantAsync(
            ("fr", "Modifier le contenu des autres B"));

        // Act and assert. The requests alternate between the tenants and the cultures, so a translation that is
        // frozen by the first request, or shared between tenants, makes a later assertion fail.
        await AssertDescriptionAsync(tenantA, "fr", "Modifier le contenu des autres A");
        await AssertDescriptionAsync(tenantB, "fr", "Modifier le contenu des autres B");
        await AssertDescriptionAsync(tenantA, "de", "Inhalte anderer bearbeiten A");
        await AssertDescriptionAsync(tenantA, "en", Description);
        await AssertDescriptionAsync(tenantB, "de", Description);
        await AssertDescriptionAsync(tenantA, "fr", "Modifier le contenu des autres A");

        Assert.Equal(Description, CommonPermissions.EditContent.Description.Value);
    }

    private static async Task<SiteContext> CreateTenantAsync(params (string Culture, string Translation)[] translations)
    {
        var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = [AdminPermissions.AccessAdminPanel, RolesPermissions.ManageRoles],
            });

        await context.InitializeAsync();
        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr", "de"]);

        foreach (var (culture, translation) in translations)
        {
            await TenantLocalizationTestHelper.WritePoFileAsync(context, culture, (s_permissionContext, Description, translation));
        }

        return context;
    }

    private static async Task AssertDescriptionAsync(SiteContext context, string culture, string description)
    {
        var page = await TenantLocalizationTestHelper.GetPageAsync(context, "Admin/Roles/Edit/Editor", culture);

        var row = Assert.Single(page.QuerySelectorAll("tr[data-text]"), element => element.QuerySelector("#Checkbox\\.EditContent") is not null);
        Assert.Equal(description, row.GetAttribute("data-text"));
        Assert.Equal(description, row.QuerySelector("td").FirstChild.TextContent.Trim());
    }
}
