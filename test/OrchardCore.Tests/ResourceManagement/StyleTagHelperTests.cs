using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using OrchardCore.ResourceManagement.TagHelpers;
using ResourceLocation = OrchardCore.ResourceManagement.ResourceLocation;

namespace OrchardCore.Tests.ResourceManagement;

public class StyleTagHelperTests : IDisposable
{
    private const string BasePath = "http://host";

    private readonly IBrowsingContext _browsingContext;

    public StyleTagHelperTests()
    {
        _browsingContext = BrowsingContext.New();
    }

    [Fact]
    public async Task AnonymousStyleWithSrcOnly_RegistersUrlWithDebugSrc()
    {
        // Arrange
        var resourceManager = CreateResourceManager();

        var tagHelper = new StyleTagHelper(resourceManager)
        {
            Src = "~/app.css",
            DebugSrc = "~/app.debug.css",
            At = ResourceLocation.Head,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var requiredResources = resourceManager.GetRequiredResources("stylesheet");
        var resource = requiredResources.FirstOrDefault()?.Resource;

        Assert.NotNull(resource);
        Assert.Equal("/app.css", resource.Url);
        Assert.Equal("/app.debug.css", resource.UrlDebug);
    }

    [Fact]
    public async Task AnonymousStyleWithSrcAndDependsOn_DefinesInInlineManifest()
    {
        // Arrange — This is the primary test for the fix in PR #18909.
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineStyle("my-dep").SetUrl("dep.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new StyleTagHelper(resourceManager)
        {
            Src = "~/app.css",
            DebugSrc = "~/app.debug.css",
            DependsOn = "my-dep",
            At = ResourceLocation.Head,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — Resource should be defined in InlineManifest with correct URLs and dependencies.
        var inlineDefinition = resourceManager.InlineManifest
            .GetResources("stylesheet")
            .FirstOrDefault(r => r.Key == "~/app.css".ToLowerInvariant())
            .Value?.FirstOrDefault();

        Assert.NotNull(inlineDefinition);
        Assert.Equal("~/app.css", inlineDefinition.Url);
        Assert.Equal("~/app.debug.css", inlineDefinition.UrlDebug);
        Assert.Contains("my-dep", inlineDefinition.Dependencies);
    }

    [Fact]
    public async Task AnonymousStyleWithDependsOn_DebugMode_RendersDebugUrl()
    {
        // Arrange
        var options = new ResourceManagementOptions { DebugMode = true };
        var manifest = new ResourceManifest();
        manifest.DefineStyle("my-dep").SetUrl("dep.css", "dep.debug.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new StyleTagHelper(resourceManager)
        {
            Src = "~/app.css",
            DebugSrc = "~/app.debug.css",
            DependsOn = "my-dep",
            At = ResourceLocation.Inline,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — Parse the rendered HTML and verify debug URLs are used.
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(output.Content.GetContent());

        var document = await ParseHtmlAsync(htmlBuilder);
        var links = document.QuerySelectorAll<IHtmlLinkElement>("link").ToList();

        Assert.Equal(2, links.Count);
        Assert.Contains(links, link => link.Href.EndsWith("dep.debug.css"));
        Assert.Contains(links, link => link.Href.EndsWith("app.debug.css"));

        // Dependency should be rendered before the dependent resource.
        Assert.Equal(DocumentPositions.Following,
            links.First(l => l.Href.EndsWith("dep.debug.css"))
                .CompareDocumentPosition(links.First(l => l.Href.EndsWith("app.debug.css"))));
    }

    [Fact]
    public async Task AnonymousStyleWithDependsOn_ReleaseMode_RendersReleaseUrl()
    {
        // Arrange
        var options = new ResourceManagementOptions { DebugMode = false };
        var manifest = new ResourceManifest();
        manifest.DefineStyle("my-dep").SetUrl("dep.css", "dep.debug.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new StyleTagHelper(resourceManager)
        {
            Src = "~/app.css",
            DebugSrc = "~/app.debug.css",
            DependsOn = "my-dep",
            At = ResourceLocation.Inline,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — Parse the rendered HTML and verify release URLs are used.
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(output.Content.GetContent());

        var document = await ParseHtmlAsync(htmlBuilder);
        var links = document.QuerySelectorAll<IHtmlLinkElement>("link").ToList();

        Assert.Equal(2, links.Count);
        Assert.Contains(links, link => link.Href.EndsWith("dep.css"));
        Assert.Contains(links, link => link.Href.EndsWith("app.css"));
    }

    [Fact]
    public async Task NamedStyleWithNameAndSrc_DefinesInInlineManifest()
    {
        // Arrange
        var resourceManager = CreateResourceManager();

        var tagHelper = new StyleTagHelper(resourceManager)
        {
            Name = "my-style",
            Src = "~/my-style.css",
            DebugSrc = "~/my-style.debug.css",
            DependsOn = "my-dep",
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — Resource should be defined in InlineManifest via PopulateResourceDefinition.
        var inlineDefinition = resourceManager.InlineManifest
            .GetResources("stylesheet")
            .FirstOrDefault(r => r.Key == "my-style")
            .Value?.FirstOrDefault();

        Assert.NotNull(inlineDefinition);
        Assert.Equal("~/my-style.css", inlineDefinition.Url);
        Assert.Equal("~/my-style.debug.css", inlineDefinition.UrlDebug);
        Assert.Contains("my-dep", inlineDefinition.Dependencies);
    }

    [Fact]
    public async Task NamedStyleRequireOnly_RegistersResource()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineStyle("existing-style").SetUrl("existing.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new StyleTagHelper(resourceManager)
        {
            Name = "existing-style",
            At = ResourceLocation.Head,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — Resource should be registered as required.
        var requiredResources = resourceManager.GetRequiredResources("stylesheet");
        var resource = requiredResources.FirstOrDefault()?.Resource;

        Assert.NotNull(resource);
        Assert.Equal("existing-style", resource.Name);
        Assert.Equal("existing.css", resource.Url);
    }

    [Fact]
    public async Task DuplicateAnonymousStylesWithDependencies_DeduplicateByName()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineStyle("my-dep").SetUrl("dep.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        // First tag helper
        var tagHelper1 = new StyleTagHelper(resourceManager)
        {
            Src = "~/foo.css",
            DebugSrc = "~/foo.debug.css",
            DependsOn = "my-dep",
            At = ResourceLocation.Head,
        };

        await tagHelper1.ProcessAsync(CreateTagHelperContext(), CreateTagHelperOutput());

        // Second tag helper with same Src
        var tagHelper2 = new StyleTagHelper(resourceManager)
        {
            Src = "~/foo.css",
            DebugSrc = "~/foo.debug.css",
            DependsOn = "my-dep",
            At = ResourceLocation.Head,
        };

        await tagHelper2.ProcessAsync(CreateTagHelperContext(), CreateTagHelperOutput());

        // Assert — Only one resource definition should exist (deduplication via Src.ToLowerInvariant()).
        var requiredResources = resourceManager.GetRequiredResources("stylesheet");
        var matchingResources = requiredResources
            .Where(ctx => ctx.Resource.Url == "~/foo.css")
            .ToList();

        Assert.Single(matchingResources);
    }

    public void Dispose() => _browsingContext?.Dispose();

    #region Helpers

    private static ResourceManager CreateResourceManager(ResourceManagementOptions options = null)
    {
        options ??= new ResourceManagementOptions();

        return new ResourceManager(
            new OptionsWrapper<ResourceManagementOptions>(options),
            StubFileVersionProvider.Instance
        );
    }

    private static TagHelperContext CreateTagHelperContext()
    {
        return new TagHelperContext(
            "style",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "style",
            new TagHelperAttributeList(),
            (useCachedResult, encoder) =>
            {
                var tagHelperContent = new DefaultTagHelperContent();
                return Task.FromResult<TagHelperContent>(tagHelperContent);
            });
    }

    private async Task<IDocument> ParseHtmlAsync(IHtmlContent content)
    {
        using var writer = new StringWriter();

        content.WriteTo(writer, System.Text.Encodings.Web.HtmlEncoder.Default);

        return await _browsingContext.OpenAsync(res => res.Content(writer.ToString()).Address(BasePath));
    }

    #endregion

    #region Stubs

    private sealed class StubFileVersionProvider : IFileVersionProvider
    {
        public static StubFileVersionProvider Instance { get; } = new StubFileVersionProvider();

        public string AddFileVersionToPath(PathString requestPathBase, string path)
        {
            return path;
        }
    }

    #endregion
}
