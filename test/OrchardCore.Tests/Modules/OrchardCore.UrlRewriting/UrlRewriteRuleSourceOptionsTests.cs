using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.UrlRewriting;
using OrchardCore.UrlRewriting.Services;

namespace OrchardCore.Tests.Modules.OrchardCore.UrlRewriting;

public class UrlRewriteRuleSourceOptionsTests
{
    [Theory]
    [InlineData(UrlRedirectRuleSource.SourceName, "Redirect", "URL Redirect Rule", typeof(UrlRedirectRuleSource))]
    [InlineData(UrlRewriteRuleSource.SourceName, "Rewrite", "URL Rewrite Rule", typeof(UrlRewriteRuleSource))]
    public void AddRewriteRuleSource_BuiltInSource_DescribesItWithItsStaticMembers(string name, string displayName, string description, Type sourceType)
    {
        // Arrange
        var services = new ServiceCollection()
            .AddOptions()
            .AddRewriteRuleSource<UrlRedirectRuleSource>(UrlRedirectRuleSource.SourceName)
            .AddRewriteRuleSource<UrlRewriteRuleSource>(UrlRewriteRuleSource.SourceName)
            .BuildServiceProvider();

        // Act
        var options = services.GetRequiredService<IOptions<UrlRewriteRuleSourceOptions>>().Value;

        // Assert
        var descriptor = Assert.Contains(name, options.Sources);
        Assert.Equal(displayName, descriptor.DisplayName.Value);
        Assert.Equal(sourceType, descriptor.DisplayName.Type);
        Assert.Equal(description, descriptor.Description.Value);
        Assert.Equal(sourceType, descriptor.Description.Type);
    }
}
