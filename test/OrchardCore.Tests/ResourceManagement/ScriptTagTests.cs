using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Fluid;
using Fluid.Ast;
using Fluid.Values;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;
using OrchardCore.ResourceManagement;
using OrchardCore.Resources.Liquid;

namespace OrchardCore.Tests.ResourceManagement;

public class ScriptTagTests : IDisposable
{
    private const string BasePath = "http://host";

    private readonly IBrowsingContext _browsingContext;

    public ScriptTagTests()
    {
        _browsingContext = BrowsingContext.New();
    }

    [Fact]
    public async Task AnonymousScriptWithSrcOnly_RegistersUrlWithDebugSrc()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.js"),
            ("debug_src", "~/app.debug.js"),
            ("at", "Foot")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

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
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.js"),
            ("debug_src", "~/app.debug.js"),
            ("depends_on", "my-dep"),
            ("at", "Foot")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

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
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        // at=Inline triggers RenderLocalScript inline.
        // at=Unspecified also triggers rendering for anonymous scripts.
        var arguments = CreateArguments(
            ("src", "~/app.js"),
            ("debug_src", "~/app.debug.js"),
            ("depends_on", "my-dep"),
            ("at", "Inline")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Parse the rendered HTML and verify debug URLs are used.
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(writer.ToString());

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
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.js"),
            ("debug_src", "~/app.debug.js"),
            ("depends_on", "my-dep"),
            ("at", "Inline")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Parse the rendered HTML and verify release URLs are used.
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(writer.ToString());

        var document = await ParseHtmlAsync(htmlBuilder);
        var scripts = document.QuerySelectorAll<IHtmlScriptElement>("script").ToList();

        Assert.Equal(2, scripts.Count);
        Assert.Contains(scripts, s => s.Source.EndsWith("dep.js"));
        Assert.Contains(scripts, s => s.Source.EndsWith("app.js"));
    }

    [Fact]
    public async Task AnonymousScriptWithSrc_UnspecifiedLocation_RendersInline()
    {
        // Arrange — When at is Unspecified, anonymous scripts render inline (same as Inline).
        var options = new ResourceManagementOptions();
        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.js")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Script should be rendered to the writer.
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(writer.ToString());

        var document = await ParseHtmlAsync(htmlBuilder);
        var scripts = document.QuerySelectorAll<IHtmlScriptElement>("script").ToList();

        Assert.Single(scripts);
        Assert.Contains(scripts, s => s.Source.EndsWith("app.js"));
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
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "bootstrap"),
            ("at", "Foot")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Resource should be registered as required.
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
        manifest.DefineScript("dep-b").SetUrl("dep-b.js");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "my-script"),
            ("depends_on", "dep-a,dep-b"),
            ("at", "Foot")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Dependencies should be resolved.
        var requiredResources = resourceManager.GetRequiredResources("script")
            .Select(ctx => ctx.Resource)
            .ToList();

        Assert.Equal(3, requiredResources.Count);
        Assert.Contains(requiredResources, r => r.Name == "dep-a");
        Assert.Contains(requiredResources, r => r.Name == "dep-b");
        Assert.Contains(requiredResources, r => r.Name == "my-script");
    }

    [Fact]
    public async Task NamedScriptWithNameAndSrc_DefinesInInlineManifest()
    {
        // Arrange
        var resourceManager = CreateResourceManager();
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "my-script"),
            ("src", "~/my-script.js"),
            ("debug_src", "~/my-script.debug.js"),
            ("depends_on", "my-dep")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

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
    public async Task NamedScriptWithNameAndSrc_AtSpecified_RegistersAndRenders()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineScript("my-dep").SetUrl("dep.js");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "my-script"),
            ("src", "~/my-script.js"),
            ("depends_on", "my-dep"),
            ("at", "Inline")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Resource registered and rendered inline.
        var requiredResources = resourceManager.GetRequiredResources("script");
        Assert.Contains(requiredResources, ctx => ctx.Resource.Name == "my-script");

        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(writer.ToString());

        var document = await ParseHtmlAsync(htmlBuilder);
        var scripts = document.QuerySelectorAll<IHtmlScriptElement>("script").ToList();

        Assert.Equal(2, scripts.Count);
        Assert.Contains(scripts, s => s.Source.EndsWith("dep.js"));
        Assert.Contains(scripts, s => s.Source.EndsWith("my-script.js"));
    }

    [Fact]
    public async Task NamedScriptWithNameAndSrc_NoAt_DoesNotRegisterOrRender()
    {
        // Arrange — When at is Unspecified, the named+src path only defines, does not register/render.
        var resourceManager = CreateResourceManager();
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "my-script"),
            ("src", "~/my-script.js")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Defined in InlineManifest but NOT registered as required.
        var inlineDefinition = resourceManager.InlineManifest
            .GetResources("script")
            .FirstOrDefault(r => r.Key == "my-script")
            .Value?.FirstOrDefault();

        Assert.NotNull(inlineDefinition);

        var requiredResources = resourceManager.GetRequiredResources("script");
        Assert.Empty(requiredResources);
        Assert.Empty(writer.ToString());
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
        var context = CreateLiquidContext(resourceManager);

        var arguments = CreateArguments(
            ("src", "~/foo.js"),
            ("debug_src", "~/foo.debug.js"),
            ("depends_on", "my-dep"),
            ("at", "Foot")
        );

        // Act — Register the same script twice.
        await ScriptTag.WriteToAsync(arguments, new StringWriter(), null, context);
        await ScriptTag.WriteToAsync(arguments, new StringWriter(), null, context);

        // Assert — Only one resource definition should exist (deduplication via src.ToLowerInvariant()).
        var requiredResources = resourceManager.GetRequiredResources("script");
        var matchingResources = requiredResources
            .Where(ctx => ctx.Resource.Url == "~/foo.js")
            .ToList();

        Assert.Single(matchingResources);
    }

    [Fact]
    public async Task AnonymousScriptWithSrc_AtFoot_DoesNotRenderInline()
    {
        // Arrange — When at=Foot, the script should be registered but NOT rendered to the writer.
        var options = new ResourceManagementOptions();
        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.js"),
            ("at", "Foot")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Nothing rendered to writer, but resource is registered.
        Assert.Empty(writer.ToString());

        var requiredResources = resourceManager.GetRequiredResources("script");
        Assert.Single(requiredResources);
    }

    [Fact]
    public async Task AnonymousScriptWithCustomAttributes_SetsAttributes()
    {
        // Arrange
        var resourceManager = CreateResourceManager();
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.js"),
            ("at", "Inline"),
            ("data-custom", "myvalue")
        );

        // Act
        await ScriptTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Custom attribute should appear in the rendered script tag.
        Assert.Contains("data-custom", writer.ToString());
        Assert.Contains("myvalue", writer.ToString());
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

    private static LiquidTemplateContext CreateLiquidContext(IResourceManager resourceManager)
    {
        var services = new ServiceCollection()
            .AddSingleton(resourceManager)
            .BuildServiceProvider();

        return new LiquidTemplateContext(services, new TemplateOptions());
    }

    private static IReadOnlyList<FilterArgument> CreateArguments(params (string name, string value)[] args)
    {
        return args.Select(a => new FilterArgument(a.name, new LiteralExpression(StringValue.Create(a.value)))).ToList();
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
