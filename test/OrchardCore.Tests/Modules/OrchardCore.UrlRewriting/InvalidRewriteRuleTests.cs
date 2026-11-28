using Microsoft.AspNetCore.Rewrite;
using OrchardCore.Admin;
using OrchardCore.Entities;
using OrchardCore.UrlRewriting;
using OrchardCore.UrlRewriting.Handlers;
using OrchardCore.UrlRewriting.Models;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.UrlRewriting;

public class InvalidRewriteRuleTests
{
    [Theory]
    [InlineData("^/old-page$", true)]
    [InlineData("^/products/(.*)", true)]
    [InlineData("^/old page$", false)]
    [InlineData("^/old-page(", false)]
    public void IsValidMatchPattern_ReturnsExpected(string pattern, bool expected)
    {
        Assert.Equal(expected, PatternHelper.IsValidMatchPattern(pattern));
    }

    [Theory]
    [InlineData("/new-page", true)]
    [InlineData("/products/$1", true)]
    [InlineData("https://%{HTTP_HOST}/new-page", true)]
    [InlineData("/new-page$", false)]
    [InlineData("/new-page%", false)]
    public void IsValidSubstitutionPattern_ReturnsExpected(string pattern, bool expected)
    {
        Assert.Equal(expected, PatternHelper.IsValidSubstitutionPattern(pattern));
    }

    [Fact]
    public async Task ValidatingAsync_InvalidSubstitutionPattern_Fails()
    {
        var handler = new UrlRedirectRuleHandler(Mock.Of<IStringLocalizer<UrlRedirectRuleHandler>>());
        var context = new ValidatingRewriteRuleContext(CreateRedirectRule("/new-page$"));

        await handler.ValidatingAsync(context);

        Assert.False(context.Result.Succeeded);
        Assert.Contains(context.Result.Errors, error => error.MemberNames.Contains(nameof(UrlRedirectSourceMetadata.SubstitutionPattern)));
    }

    [Fact]
    public void Configure_InvalidRule_IsSkipped()
    {
        var rulesStore = new Mock<IRewriteRulesStore>();
        rulesStore.Setup(store => store.GetAllAsync()).ReturnsAsync(
        [
            CreateRedirectRule("/new-page$", order: 1),
            CreateRedirectRule("/new-page", order: 2),
        ]);

        var serviceProvider = new ServiceCollection()
            .AddKeyedSingleton<IUrlRewriteRuleSource>(UrlRedirectRuleSource.SourceName, new UrlRedirectRuleSource())
            .BuildServiceProvider();

        var configuration = new RewriteOptionsConfiguration(
            rulesStore.Object,
            serviceProvider,
            Options.Create(new AdminOptions()),
            NullLogger<RewriteOptionsConfiguration>.Instance);

        var options = new RewriteOptions();

        configuration.Configure(options);

        // The admin prefix exclusion rule and the valid redirect rule.
        Assert.Equal(2, options.Rules.Count);
    }

    private static RewriteRule CreateRedirectRule(string substitutionPattern, int order = 0)
    {
        var rule = new RewriteRule
        {
            Name = substitutionPattern,
            Source = UrlRedirectRuleSource.SourceName,
            Order = order,
        };

        rule.Put(new UrlRedirectSourceMetadata
        {
            Pattern = "^/old-page$",
            SubstitutionPattern = substitutionPattern,
            RedirectType = RedirectType.MovedPermanently,
            QueryStringPolicy = QueryStringPolicy.Append,
        });

        return rule;
    }
}
