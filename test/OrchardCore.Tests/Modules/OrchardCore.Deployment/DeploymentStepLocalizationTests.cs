using AngleSharp.Html.Dom;
using OrchardCore.Admin;
using OrchardCore.Deployment;
using OrchardCore.Deployment.Steps;
using OrchardCore.Security.Permissions;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.Tests.Localization;
using ISession = YesSql.ISession;

namespace OrchardCore.Tests.Modules.OrchardCore.Deployment;

/// <summary>
/// Renders the deployment pages of two tenants in several cultures, and checks that the category and the title
/// of a step use the PO file translations of the tenant, in the culture of each request.
/// </summary>
public class DeploymentStepLocalizationTests
{
    private static readonly string s_stepContext = typeof(CustomFileDeploymentStep).FullName;

    [Fact]
    public async Task StepTitleAndCategory_TwoTenantsAndCultures_UseTheTranslationsOfEachTenantAndCulture()
    {
        // Arrange
        using var tenantA = await CreateTenantAsync(
            ("fr", [(s_stepContext, "Custom File", "Fichier personnalisé A"), (s_stepContext, "Deployment", "Déploiement A")]),
            ("de", [(s_stepContext, "Custom File", "Benutzerdefinierte Datei A"), (s_stepContext, "Deployment", "Bereitstellung A")]));

        using var tenantB = await CreateTenantAsync(
            ("fr", [(s_stepContext, "Custom File", "Fichier personnalisé B"), (s_stepContext, "Deployment", "Déploiement B")]));

        var planA = await CreatePlanAsync(tenantA);
        var planB = await CreatePlanAsync(tenantB);

        // Act and assert. The same step type is rendered by two tenants and in three cultures. The requests
        // alternate between the tenants and the cultures, so a translation that is frozen by the first request,
        // or shared between tenants, makes a later assertion fail.
        await AssertStepAsync(tenantA, planA, "fr", "Fichier personnalisé A", "Déploiement A");
        await AssertStepAsync(tenantB, planB, "fr", "Fichier personnalisé B", "Déploiement B");
        await AssertStepAsync(tenantA, planA, "de", "Benutzerdefinierte Datei A", "Bereitstellung A");
        await AssertStepAsync(tenantA, planA, "en", "Custom File", "Deployment");
        await AssertStepAsync(tenantB, planB, "de", "Custom File", "Deployment");
        await AssertStepAsync(tenantA, planA, "fr", "Fichier personnalisé A", "Déploiement A");
    }

    private static async Task<SiteContext> CreateTenantAsync(params (string Culture, (string Context, string Text, string Translation)[] Entries)[] poFiles)
    {
        var context = new SiteContext()
            .WithPermissionsContext(new PermissionsContext
            {
                UsePermissionsContext = true,
                AuthorizedPermissions = [AdminPermissions.AccessAdminPanel, DeploymentPermissions.ManageDeploymentPlan, DeploymentPermissions.Export],
            });

        await context.InitializeAsync();
        await TenantLocalizationTestHelper.EnableLocalizationAsync(context, ["en", "fr", "de"], "OrchardCore.Deployment");

        foreach (var (culture, entries) in poFiles)
        {
            await TenantLocalizationTestHelper.WritePoFileAsync(context, culture, entries);
        }

        return context;
    }

    private static async Task<DeploymentPlan> CreatePlanAsync(SiteContext context)
    {
        var plan = new DeploymentPlan { Name = "Localization plan" };
        await context.UsingTenantScopeAsync(scope => scope.ServiceProvider.GetRequiredService<ISession>().SaveAsync(plan));

        return plan;
    }

    private static async Task AssertStepAsync(SiteContext context, DeploymentPlan plan, string culture, string title, string category)
    {
        // The title is the heading of the screen that adds the step.
        var createPage = await TenantLocalizationTestHelper.GetPageAsync(
            context, $"Admin/DeploymentPlan/{plan.Id}/Step/Create?type={nameof(CustomFileDeploymentStep)}", culture);
        var heading = Assert.Single(createPage.QuerySelectorAll("h1.oc-breadcrumb-title"));
        Assert.Equal(title, heading.TextContent);

        // The category groups the step in the list of steps of the plan.
        var planPage = await TenantLocalizationTestHelper.GetPageAsync(context, $"Admin/DeploymentPlan/Display/{plan.Id}", culture);
        var categoryId = GetStepCategoryId(planPage);
        var categoryLink = Assert.Single(planPage.QuerySelectorAll($"a[data-list-filter-value='{categoryId}']"));
        Assert.Equal(category, categoryLink.TextContent);
    }

    private static string GetStepCategoryId(IHtmlDocument page)
    {
        var item = Assert.Single(
            page.QuerySelectorAll("div.deployment-step-item"),
            element => element.QuerySelector($"a[href*='type={nameof(CustomFileDeploymentStep)}']") is not null);

        return item.GetAttribute("data-category");
    }
}
