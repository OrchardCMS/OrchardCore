using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.AspNetCore.TestHost;
using OrchardCore.UrlRewriting.Extensions;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Hosting;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting;
using OrchardCore.Environment.Shell;
using Microsoft.Extensions.Logging.Abstractions;
using OrchardCore.Localization;
using OrchardCore.UrlRewriting.Handlers;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.Tests.Apis.RemoteManagement;

public class UrlRewriteManagementTests
{
    [Fact]
    public async Task RuntimeRewrite_ReselectsAnEndpointAfterInitialRouting()
    {
        var rule = new RewriteRule { Source = "Rewrite" };
        rule.Put(new UrlRewriteSourceMetadata { Pattern = "^/source$", SubstitutionPattern = "/target", SkipFurtherRules = true });
        using var host = await new HostBuilder().ConfigureWebHost(web => web.UseTestServer()
            .ConfigureServices(services =>
            {
                services.AddRouting();
                services.Configure<RewriteOptions>(options => new UrlRewriteRuleSource(
                    new StringLocalizer<UrlRewriteRuleSource>(new NullStringLocalizerFactory())).Configure(options, rule));
            })
            .Configure(app =>
            {
                app.UseRouting();
                app.UseUrlRewriting(app.ApplicationServices);
                Assert.False(app.Properties.ContainsKey("__GlobalEndpointRouteBuilder"));
                app.UseEndpoints(routes =>
                {
                    routes.MapGet("/source", () => "original endpoint");
                    routes.MapGet("/target", () => "rewritten endpoint");
                });
            })).StartAsync(TestContext.Current.CancellationToken);
        using var client = host.GetTestClient();
        Assert.Equal("rewritten endpoint", await client.GetStringAsync("/source", TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task ManagerValidation_ConstructsTheRegisteredRuntimeSource()
    {
        var source = new Mock<IUrlRewriteRuleSource>();
        source.Setup(value => value.Configure(It.IsAny<global::Microsoft.AspNetCore.Rewrite.RewriteOptions>(), It.IsAny<RewriteRule>()))
            .Throws(new FormatException("Invalid runtime expression"));
        using var services = new ServiceCollection().AddKeyedSingleton("Test", source.Object).BuildServiceProvider();
        var manager = new RewriteRulesManager(Mock.Of<IRewriteRulesStore>(), [], services,
            NullLogger<RewriteRulesManager>.Instance,
            new StringLocalizer<RewriteRulesManager>(new NullStringLocalizerFactory()), Mock.Of<IShellReleaseManager>());
        var result = await manager.ValidateAsync(new RewriteRule { Id = "test", Name = "Test", Source = "Test" });
        Assert.False(result.Succeeded);
        Assert.Single(result.Errors);
        source.Verify(value => value.Configure(It.IsAny<global::Microsoft.AspNetCore.Rewrite.RewriteOptions>(), It.IsAny<RewriteRule>()), Times.Once);
    }

    [Fact]
    public async Task SaveAndDelete_IdenticalRetries_DoNotReloadAgain()
    {
        var rule = new RewriteRule { Id = "rule", Name = "Original", Source = "Rewrite" };
        rule.Put(new UrlRewriteSourceMetadata { Pattern = "^old$", SubstitutionPattern = "/target" });
        var store = new Mock<IRewriteRulesStore>();
        RewriteRule saved = rule.Clone();
        store.Setup(service => service.FindByIdAsync(rule.Id)).ReturnsAsync(() => saved);
        store.Setup(service => service.SaveAsync(It.IsAny<RewriteRule>()))
            .Callback((RewriteRule value) => saved = value.Clone()).Returns(Task.CompletedTask);
        store.Setup(service => service.DeleteAsync(It.IsAny<RewriteRule>()))
            .Callback(() => saved = null).Returns(Task.CompletedTask);
        var release = new Mock<IShellReleaseManager>();
        var manager = new RewriteRulesManager(store.Object, [], Mock.Of<IServiceProvider>(), NullLogger<RewriteRulesManager>.Instance,
            new StringLocalizer<RewriteRulesManager>(new NullStringLocalizerFactory()), release.Object);
        await manager.SaveAsync(rule.Clone());
        release.Verify(service => service.RequestRelease(), Times.Never);
        var changed = rule.Clone();
        changed.Name = "Changed";
        await manager.SaveAsync(changed);
        await manager.SaveAsync(changed.Clone());
        release.Verify(service => service.RequestRelease(), Times.Once);
        store.Verify(service => service.SaveAsync(It.IsAny<RewriteRule>()), Times.Once);
        await manager.DeleteAsync(changed);
        await manager.DeleteAsync(changed);
        release.Verify(service => service.RequestRelease(), Times.Exactly(2));
        store.Verify(service => service.DeleteAsync(It.IsAny<RewriteRule>()), Times.Once);
    }

    [Fact]
    public async Task Reorder_OneBasedAdminIndexes_MoveFirstRuleToLast()
    {
        RewriteRule[] rules = [new() { Id = "first", Order = 0 }, new() { Id = "second", Order = 1 }];
        var store = new Mock<IRewriteRulesStore>();
        store.Setup(service => service.GetAllAsync()).ReturnsAsync(rules);
        var manager = new RewriteRulesManager(store.Object, [], Mock.Of<IServiceProvider>(), NullLogger<RewriteRulesManager>.Instance,
            new StringLocalizer<RewriteRulesManager>(new NullStringLocalizerFactory()), Mock.Of<IShellReleaseManager>());
        await manager.ResortOrderAsync(1, 2);
        store.Verify(service => service.UpdateOrderAndSaveAsync(It.Is<IEnumerable<RewriteRule>>(items =>
            items.Select(item => item.Id).SequenceEqual(new[] { "second", "first" }))), Times.Once);
    }

    [Fact]
    public void Clone_ChangingMetadata_DoesNotChangeStoredRule()
    {
        var original = new RewriteRule { Id = "original", Source = "Rewrite", Name = "Original" };
        original.Put(new UrlRewriteSourceMetadata { Pattern = "^before$", SubstitutionPattern = "/target" });
        var clone = original.Clone();
        clone.Put(new UrlRewriteSourceMetadata { Pattern = "^after$", SubstitutionPattern = "/target" });
        Assert.Equal("^before$", original.GetOrCreate<UrlRewriteSourceMetadata>().Pattern);
    }

    [Theory]
    [InlineData("^old$", "/target", 0, true)]
    [InlineData("^old$", "/target", 42, false)]
    [InlineData("^old$", "/target\nRewriteRule .* /other [R=302]", 0, false)]
    public async Task Handler_ValidatesRuntimeInput(string pattern, string substitution, int queryPolicy, bool valid)
    {
        var rule = new RewriteRule { Source = UrlRewriteRuleSource.SourceName, Name = "Test" };
        rule.Put(new UrlRewriteSourceMetadata
        {
            Pattern = pattern, SubstitutionPattern = substitution,
            QueryStringPolicy = (QueryStringPolicy)queryPolicy,
        });
        var context = new ValidatingRewriteRuleContext(rule);
        await new UrlRewriteRuleHandler(new StringLocalizer<UrlRewriteRuleHandler>(new NullStringLocalizerFactory())).ValidatingAsync(context);
        Assert.Equal(valid, context.Result.Succeeded);
    }

    [Fact]
    public async Task Handler_OptionalInitializationData_DoesNotThrow()
    {
        var rule = new RewriteRule { Source = UrlRewriteRuleSource.SourceName };
        var handler = new UrlRewriteRuleHandler(new StringLocalizer<UrlRewriteRuleHandler>(new NullStringLocalizerFactory()));
        await handler.InitializingAsync(new InitializingRewriteRuleContext(rule, null));
    }
}
