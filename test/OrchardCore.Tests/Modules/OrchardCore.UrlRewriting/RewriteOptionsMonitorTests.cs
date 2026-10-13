using System.Text.Json.Nodes;
using OrchardCore.Tests.Apis.Context;
using OrchardCore.UrlRewriting;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.UrlRewriting;

public class RewriteOptionsMonitorTests
{
    private const string SourcePath = "about-us";

    [Fact]
    public async Task SaveRule_WithRulesManager_AppliesRedirectWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await GetSourceAsync(context)).StatusCode);

        var shellTicks = await context.GetShellTicksAsync();

        await SaveRedirectRuleAsync(context, "/about");

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        AssertRedirectsTo(await GetSourceAsync(context), "/about");
    }

    [Fact]
    public async Task DeleteRule_WithRulesManager_RemovesRedirectWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        var ruleId = await SaveRedirectRuleAsync(context, "/about");

        // Make sure that the rule is applied before deleting it, regardless of the update notifications.
        await context.ReleaseShellAsync();

        AssertRedirectsTo(await GetSourceAsync(context), "/about");

        var shellTicks = await context.GetShellTicksAsync();

        // The bulk remove action deletes the rules through the rules manager.
        await context.UsingTenantScopeAsync(async scope =>
        {
            var rulesManager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();

            await rulesManager.DeleteAsync(await rulesManager.FindByIdAsync(ruleId));
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        Assert.Equal(HttpStatusCode.NotFound, (await GetSourceAsync(context)).StatusCode);
    }

    [Fact]
    public async Task ResortRules_WithRulesManager_AppliesNewOrderWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        await SaveRedirectRuleAsync(context, "/first");
        await SaveRedirectRuleAsync(context, "/second");

        // Make sure that the rules are applied before sorting them, regardless of the update notifications.
        await context.ReleaseShellAsync();

        AssertRedirectsTo(await GetSourceAsync(context), "/first");

        var shellTicks = await context.GetShellTicksAsync();

        // The sort endpoint reorders the rules through the rules manager.
        await context.UsingTenantScopeAsync(scope =>
            scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>().ResortOrderAsync(2, 1));

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        AssertRedirectsTo(await GetSourceAsync(context), "/second");
    }

    [Fact]
    public async Task ImportRules_WithRecipeStep_AppliesRedirectWithoutReleasingTenant()
    {
        using var context = await CreateContextAsync();

        Assert.Equal(HttpStatusCode.NotFound, (await GetSourceAsync(context)).StatusCode);

        var shellTicks = await context.GetShellTicksAsync();

        await context.ExecuteRecipeStepAsync(new JsonObject
        {
            ["name"] = "UrlRewriting",
            ["Rules"] = new JsonArray(CreateRedirectRuleData("/about")),
        });

        Assert.Equal(shellTicks, await context.GetShellTicksAsync());
        AssertRedirectsTo(await GetSourceAsync(context), "/about");
    }

    private static async Task<SiteContext> CreateContextAsync()
    {
        var context = new SiteContext()
            .WithRecipe("SaaS");

        await context.InitializeAsync();
        await context.EnableFeaturesAsync("OrchardCore.UrlRewriting");

        return context;
    }

    private static async Task<string> SaveRedirectRuleAsync(SiteContext context, string target)
    {
        string ruleId = null;

        await context.UsingTenantScopeAsync(async scope =>
        {
            var rulesManager = scope.ServiceProvider.GetRequiredService<IRewriteRulesManager>();

            var rule = await rulesManager.NewAsync(UrlRedirectRuleSource.SourceName, CreateRedirectRuleData(target));

            Assert.True((await rulesManager.ValidateAsync(rule)).Succeeded);

            await rulesManager.SaveAsync(rule);

            ruleId = rule.Id;
        });

        await context.WaitForDeferredTasksAsync(CancellationToken.None);

        return ruleId;
    }

    private static JsonObject CreateRedirectRuleData(string target)
        => new()
        {
            [nameof(RewriteRule.Source)] = UrlRedirectRuleSource.SourceName,
            [nameof(RewriteRule.Name)] = $"Redirect to {target}",
            [nameof(UrlRedirectSourceMetadata.Pattern)] = $"^/?{SourcePath}$",
            [nameof(UrlRedirectSourceMetadata.SubstitutionPattern)] = target,
            [nameof(UrlRedirectSourceMetadata.RedirectType)] = nameof(RedirectType.Found),
        };

    private static Task<HttpResponseMessage> GetSourceAsync(SiteContext context)
        => context.Client.GetAsync(SourcePath, TestContext.Current.CancellationToken);

    private static void AssertRedirectsTo(HttpResponseMessage response, string target)
    {
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.EndsWith(target, response.Headers.Location.OriginalString);
    }
}
