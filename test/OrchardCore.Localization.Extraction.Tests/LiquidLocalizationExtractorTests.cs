using Fluid;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Liquid;
using Xunit;

namespace OrchardCore.Localization.Extraction.Tests;

public sealed class LiquidLocalizationExtractorTests
{
    [Theory]
    [InlineData("{% zone 'Content' %}{% if true %}{{ 'Nested' | t }}{% endif %}{% endzone %}")]
    [InlineData("{% cache 'Key' %}{% for x in (1..2) %}{{ 'Nested' | t }}{% endfor %}{% endcache %}")]
    [InlineData("{% a href: '/test' %}{{ 'Nested' | t }}{% enda %}")]
    [InlineData("{% form 'post' %}{{ 'Nested' | t }}{% endform %}")]
    public void Extract_OrchardBlocks_VisitsNestedMessages(string source)
    {
        using var workspace = new TestWorkspace();
        var options = workspace.Options("Theme");
        var catalog = new LocalizationCatalog();
        new LiquidLocalizationExtractor().Extract(source, new ExtractionFile(Path.Combine(workspace.DirectoryPath, "Views/Test.liquid"), "Views/Test.liquid"), options, catalog, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
        var message = Assert.Single(catalog.Messages);
        Assert.Equal("Nested", message.Text);
        Assert.Equal("Theme.Views.Test", message.Context);
        Assert.Equal(0, Assert.Single(message.References).Line);
    }

    [Theory]
    [InlineData("{% resources type: 'Meta' %}")]
    [InlineData("{% script name: 'test', at: 'Foot' %}")]
    public void Extract_ResourceTags_DoesNotFailParsing(string source)
    {
        using var workspace = new TestWorkspace();
        var catalog = new LocalizationCatalog();
        new LiquidLocalizationExtractor().Extract(source, new ExtractionFile("Views/Test.liquid", "Views/Test.liquid"), workspace.Options(), catalog, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
    }

    [Theory]
    [InlineData("{% layout 'Title' | t %}")]
    [InlineData("{% page_title title: ('Title' | t) %}")]
    [InlineData("{% shape 'Test', title: ('Title' | t) %}")]
    [InlineData("{% zone 'Content', title: ('Title' | t) %}{% endzone %}")]
    public void Extract_ParserArguments_VisitsLocalizationFilters(string source)
    {
        using var workspace = new TestWorkspace();
        var catalog = new LocalizationCatalog();
        var parser = new LiquidViewParser(Options.Create(new LiquidViewOptions()), Options.Create(new FluidParserOptions { AllowParentheses = true }));
        new LiquidLocalizationExtractor(parser).Extract(source, new ExtractionFile("Views/Test.liquid", "Views/Test.liquid"), workspace.Options(), catalog, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
        Assert.Equal("Title", Assert.Single(catalog.Messages).Text);
    }

    [Fact]
    public void Extract_CustomParser_VisitsExpressionsWithoutTagSpecificRules()
    {
        using var workspace = new TestWorkspace();
        var parser = new FluidParser();
        parser.RegisterExpressionTag("custom", (_, _, _, _) => throw new InvalidOperationException("Extraction must not render templates."));
        var catalog = new LocalizationCatalog();
        new LiquidLocalizationExtractor(parser).Extract("{% custom 'Custom title' | t %}", new ExtractionFile("Views/Test.liquid", "Views/Test.liquid"), workspace.Options(), catalog, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
        Assert.Equal("Custom title", Assert.Single(catalog.Messages).Text);
    }

    [Fact]
    public void Extract_RawAndComments_IgnoresNonExecutableExpressions()
    {
        using var workspace = new TestWorkspace();
        var catalog = new LocalizationCatalog();
        new LiquidLocalizationExtractor().Extract("{% raw %}{{ 'Raw' | t }}{% endraw %}{% comment %}{{ 'Comment' | t }}{% endcomment %}{{ 'Visible' | t }}", new ExtractionFile("Views/Test.liquid", "Views/Test.liquid"), workspace.Options(), catalog, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
        Assert.Equal("Visible", Assert.Single(catalog.Messages).Text);
    }

    [Fact]
    public void Extract_DynamicOrTransformedInput_ReportsWithoutGuessing()
    {
        using var workspace = new TestWorkspace();
        var catalog = new LocalizationCatalog();
        new LiquidLocalizationExtractor().Extract("{{ Model.Label | t }}{{ 'Changed' | append: ' suffix' | t }}{{ 'Real key' | t | escape }}", new ExtractionFile("Views/Test.liquid", "Views/Test.liquid"), workspace.Options(), catalog, TestContext.Current.CancellationToken);
        Assert.Equal("Real key", Assert.Single(catalog.Messages).Text);
        Assert.Equal(2, catalog.Diagnostics.Count);
        Assert.All(catalog.Diagnostics, diagnostic => Assert.Equal("OCLOC001", diagnostic.Code));
    }

    [Fact]
    public void Extract_UnknownCustomTag_ReportsParseError()
    {
        using var workspace = new TestWorkspace();
        var catalog = new LocalizationCatalog();
        new LiquidLocalizationExtractor().Extract("{% unknown_custom_tag %}", new ExtractionFile("Views/Test.liquid", "Views/Test.liquid"), workspace.Options(), catalog, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Messages);
        Assert.True(Assert.Single(catalog.Diagnostics).IsError);
    }

    [Fact]
    public void Extract_ConfiguredFilterAlias_UsesExplicitAdapter()
    {
        using var workspace = new TestWorkspace();
        var catalog = new LocalizationCatalog();
        new LiquidLocalizationExtractor(localizationFilters: ["translate"]).Extract("{{ 'Alias key' | translate }}", new ExtractionFile("Views/Test.liquid", "Views/Test.liquid"), workspace.Options(), catalog, TestContext.Current.CancellationToken);
        Assert.Empty(catalog.Diagnostics);
        Assert.Equal("Alias key", Assert.Single(catalog.Messages).Text);
    }
}
