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

public class ScriptTagHelperTests : IDisposable
{
    private const string BasePath = "http://host";

    private readonly IBrowsingContext _browsingContext;

    public ScriptTagHelperTests()
    {
        _browsingContext = BrowsingContext.New();
    }

    [Fact]
    public async Task AnonymousScriptWithSrcOnly_RegistersUrlWithDebugSrc()
    {
        // Arrange
        var resourceManager = CreateResourceManager();

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Src = "~/app.js",
            DebugSrc = "~/app.debug.js",
            At = ResourceLocation.Foot,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var requiredResources = resourceManager.GetRequiredResources("script");
        var resource = requiredResources.FirstOrDefault()?.Resource;

        Assert.NotNull(resource);
        Assert.Equal("/app.js", resource.Url);
        Assert.Equal("/app.debug.js", resource.UrlDebug);
    }

    [Fact]
    public async Task AnonymousScriptWithSrcAndDependsOn_DefinesInInlineManifest()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineScript("my-dep").SetUrl("dep.js");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Src = "~/app.js",
            DebugSrc = "~/app.debug.js",
            DependsOn = "my-dep",
            At = ResourceLocation.Foot,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — Resource should be defined in InlineManifest with correct URLs and dependencies.
        var inlineDefinition = resourceManager.InlineManifest
            .GetResources("script")
            .FirstOrDefault(r => r.Key == "~/app.js".ToLowerInvariant())
            .Value?.FirstOrDefault();

        Assert.NotNull(inlineDefinition);
        Assert.Equal("~/app.js", inlineDefinition.Url);
        Assert.Equal("~/app.debug.js", inlineDefinition.UrlDebug);
        Assert.Contains("my-dep", inlineDefinition.Dependencies);
    }

    [Fact]
    public async Task AnonymousScriptWithDependsOn_DebugMode_RendersDebugUrl()
    {
        // Arrange
        var options = new ResourceManagementOptions { DebugMode = true };
        var manifest = new ResourceManifest();
        manifest.DefineScript("my-dep").SetUrl("dep.js", "dep.debug.js");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Src = "~/app.js",
            DebugSrc = "~/app.debug.js",
            DependsOn = "my-dep",
            At = ResourceLocation.Inline,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(output.Content.GetContent());

        var document = await ParseHtmlAsync(htmlBuilder);
        var scripts = document.QuerySelectorAll<IHtmlScriptElement>("script").ToList();

        Assert.Equal(2, scripts.Count);
        Assert.Contains(scripts, s => s.Source.EndsWith("dep.debug.js"));
        Assert.Contains(scripts, s => s.Source.EndsWith("app.debug.js"));

        // Dependency should be rendered before the dependent resource.
        Assert.Equal(DocumentPositions.Following,
            scripts.First(s => s.Source.EndsWith("dep.debug.js"))
                .CompareDocumentPosition(scripts.First(s => s.Source.EndsWith("app.debug.js"))));
    }

    [Fact]
    public async Task AnonymousScriptWithDependsOn_ReleaseMode_RendersReleaseUrl()
    {
        // Arrange
        var options = new ResourceManagementOptions { DebugMode = false };
        var manifest = new ResourceManifest();
        manifest.DefineScript("my-dep").SetUrl("dep.js", "dep.debug.js");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Src = "~/app.js",
            DebugSrc = "~/app.debug.js",
            DependsOn = "my-dep",
            At = ResourceLocation.Inline,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(output.Content.GetContent());

        var document = await ParseHtmlAsync(htmlBuilder);
        var scripts = document.QuerySelectorAll<IHtmlScriptElement>("script").ToList();

        Assert.Equal(2, scripts.Count);
        Assert.Contains(scripts, s => s.Source.EndsWith("dep.js"));
        Assert.Contains(scripts, s => s.Source.EndsWith("app.js"));
    }

    [Fact]
    public async Task AnonymousScriptWithSrc_UnspecifiedLocation_RendersInline()
    {
        // Arrange — ScriptTagHelper renders when At is Unspecified (unlike StyleTagHelper).
        var resourceManager = CreateResourceManager();

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Src = "~/app.js",
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(output.Content.GetContent());

        var document = await ParseHtmlAsync(htmlBuilder);
        var scripts = document.QuerySelectorAll<IHtmlScriptElement>("script").ToList();

        Assert.Single(scripts);
        Assert.Contains(scripts, s => s.Source.EndsWith("app.js"));
    }

    [Fact]
    public async Task AnonymousScriptWithSrc_AtFoot_DoesNotRenderInline()
    {
        // Arrange — When at=Foot, the script is registered but NOT rendered to output.
        var resourceManager = CreateResourceManager();

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Src = "~/app.js",
            At = ResourceLocation.Foot,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        Assert.Empty(output.Content.GetContent());

        var requiredResources = resourceManager.GetRequiredResources("script");
        Assert.Single(requiredResources);
    }

    [Fact]
    public async Task NamedScriptWithNameAndSrc_DefinesInInlineManifest()
    {
        // Arrange
        var resourceManager = CreateResourceManager();

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Name = "my-script",
            Src = "~/my-script.js",
            DebugSrc = "~/my-script.debug.js",
            DependsOn = "my-dep",
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert — Resource should be defined in InlineManifest.
        var inlineDefinition = resourceManager.InlineManifest
            .GetResources("script")
            .FirstOrDefault(r => r.Key == "my-script")
            .Value?.FirstOrDefault();

        Assert.NotNull(inlineDefinition);
        Assert.Equal("~/my-script.js", inlineDefinition.Url);
        Assert.Equal("~/my-script.debug.js", inlineDefinition.UrlDebug);
        Assert.Contains("my-dep", inlineDefinition.Dependencies);
    }

    [Fact]
    public async Task NamedScriptWithNameAndSrc_NoAt_DoesNotRegisterOrRender()
    {
        // Arrange — When At is Unspecified, the named+src path only defines, does not register.
        var resourceManager = CreateResourceManager();

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Name = "my-script",
            Src = "~/my-script.js",
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var inlineDefinition = resourceManager.InlineManifest
            .GetResources("script")
            .FirstOrDefault(r => r.Key == "my-script")
            .Value?.FirstOrDefault();

        Assert.NotNull(inlineDefinition);

        var requiredResources = resourceManager.GetRequiredResources("script");
        Assert.Empty(requiredResources);
        Assert.Empty(output.Content.GetContent());
    }

    [Fact]
    public async Task NamedScriptRequireOnly_RegistersResource()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineScript("bootstrap").SetUrl("bootstrap.js");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Name = "bootstrap",
            At = ResourceLocation.Foot,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var requiredResources = resourceManager.GetRequiredResources("script");
        var resource = requiredResources.FirstOrDefault()?.Resource;

        Assert.NotNull(resource);
        Assert.Equal("bootstrap", resource.Name);
        Assert.Equal("bootstrap.js", resource.Url);
    }

    [Fact]
    public async Task NamedScriptRequireOnly_WithDependsOn_AddsDependencies()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineScript("my-script").SetUrl("my-script.js");
        manifest.DefineScript("dep-a").SetUrl("dep-a.js");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        var tagHelper = new ScriptTagHelper(resourceManager)
        {
            Name = "my-script",
            DependsOn = "dep-a",
            At = ResourceLocation.Foot,
        };

        var output = CreateTagHelperOutput();
        var context = CreateTagHelperContext();

        // Act
        await tagHelper.ProcessAsync(context, output);

        // Assert
        var requiredResources = resourceManager.GetRequiredResources("script")
            .Select(ctx => ctx.Resource)
            .ToList();

        Assert.Equal(2, requiredResources.Count);
        Assert.Contains(requiredResources, r => r.Name == "dep-a");
        Assert.Contains(requiredResources, r => r.Name == "my-script");
    }

    [Fact]
    public async Task DuplicateAnonymousScriptsWithDependencies_DeduplicateByName()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineScript("my-dep").SetUrl("dep.js");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);

        // First tag helper
        var tagHelper1 = new ScriptTagHelper(resourceManager)
        {
            Src = "~/foo.js",
            DebugSrc = "~/foo.debug.js",
            DependsOn = "my-dep",
            At = ResourceLocation.Foot,
        };

        await tagHelper1.ProcessAsync(CreateTagHelperContext(), CreateTagHelperOutput());

        // Second tag helper with same Src
        var tagHelper2 = new ScriptTagHelper(resourceManager)
        {
            Src = "~/foo.js",
            DebugSrc = "~/foo.debug.js",
            DependsOn = "my-dep",
            At = ResourceLocation.Foot,
        };

        await tagHelper2.ProcessAsync(CreateTagHelperContext(), CreateTagHelperOutput());

        // Assert — Only one resource definition should exist.
        var requiredResources = resourceManager.GetRequiredResources("script");
        var matchingResources = requiredResources
            .Where(ctx => ctx.Resource.Url == "~/foo.js")
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
            "script",
            new TagHelperAttributeList(),
            new Dictionary<object, object>(),
            Guid.NewGuid().ToString("N"));
    }

    private static TagHelperOutput CreateTagHelperOutput()
    {
        return new TagHelperOutput(
            "script",
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
