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

public class StyleTagTests : IDisposable
{
    private const string BasePath = "http://host";

    private readonly IBrowsingContext _browsingContext;

    public StyleTagTests()
    {
        _browsingContext = BrowsingContext.New();
    }

    [Fact]
    public async Task AnonymousStyleWithSrcOnly_RegistersUrlWithDebugSrc()
    {
        // Arrange
        var resourceManager = CreateResourceManager();
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.css"),
            ("debug_src", "~/app.debug.css"),
            ("at", "Head")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

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
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineStyle("my-dep").SetUrl("dep.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.css"),
            ("debug_src", "~/app.debug.css"),
            ("depends_on", "my-dep"),
            ("at", "Head")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

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
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.css"),
            ("debug_src", "~/app.debug.css"),
            ("depends_on", "my-dep"),
            ("at", "Inline")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

        // Assert
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(writer.ToString());

        var document = await ParseHtmlAsync(htmlBuilder);
        var links = document.QuerySelectorAll<IHtmlLinkElement>("link").ToList();

        Assert.Equal(2, links.Count);
        Assert.Contains(links, l => l.Href.EndsWith("dep.debug.css"));
        Assert.Contains(links, l => l.Href.EndsWith("app.debug.css"));

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
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.css"),
            ("debug_src", "~/app.debug.css"),
            ("depends_on", "my-dep"),
            ("at", "Inline")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

        // Assert
        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(writer.ToString());

        var document = await ParseHtmlAsync(htmlBuilder);
        var links = document.QuerySelectorAll<IHtmlLinkElement>("link").ToList();

        Assert.Equal(2, links.Count);
        Assert.Contains(links, l => l.Href.EndsWith("dep.css"));
        Assert.Contains(links, l => l.Href.EndsWith("app.css"));
    }

    [Fact]
    public async Task AnonymousStyleWithSrc_AtHead_DoesNotRenderInline()
    {
        // Arrange — StyleTag only renders inline when at=Inline (unlike ScriptTag).
        var resourceManager = CreateResourceManager();
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("src", "~/app.css"),
            ("at", "Head")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Nothing rendered to writer, but resource is registered.
        Assert.Empty(writer.ToString());

        var requiredResources = resourceManager.GetRequiredResources("stylesheet");
        Assert.Single(requiredResources);
    }

    [Fact]
    public async Task NamedStyleRequireOnly_RegistersResource()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineStyle("bootstrap-css").SetUrl("bootstrap.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "bootstrap-css"),
            ("at", "Head")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

        // Assert
        var requiredResources = resourceManager.GetRequiredResources("stylesheet");
        var resource = requiredResources.FirstOrDefault()?.Resource;

        Assert.NotNull(resource);
        Assert.Equal("bootstrap-css", resource.Name);
        Assert.Equal("bootstrap.css", resource.Url);
    }

    [Fact]
    public async Task NamedStyleRequireOnly_WithDependsOn_AddsDependencies()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineStyle("my-style").SetUrl("my-style.css");
        manifest.DefineStyle("dep-a").SetUrl("dep-a.css");
        manifest.DefineStyle("dep-b").SetUrl("dep-b.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "my-style"),
            ("depends_on", "dep-a,dep-b"),
            ("at", "Head")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Dependencies should be resolved.
        var requiredResources = resourceManager.GetRequiredResources("stylesheet")
            .Select(ctx => ctx.Resource)
            .ToList();

        Assert.Equal(3, requiredResources.Count);
        Assert.Contains(requiredResources, r => r.Name == "dep-a");
        Assert.Contains(requiredResources, r => r.Name == "dep-b");
        Assert.Contains(requiredResources, r => r.Name == "my-style");
    }

    [Fact]
    public async Task NamedStyleWithNameAndSrc_DefinesInInlineManifest()
    {
        // Arrange
        var resourceManager = CreateResourceManager();
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "my-style"),
            ("src", "~/my-style.css"),
            ("debug_src", "~/my-style.debug.css"),
            ("depends_on", "my-dep")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Resource should be defined in InlineManifest.
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
    public async Task NamedStyleWithNameAndSrc_AtSpecified_RegistersAndRenders()
    {
        // Arrange
        var options = new ResourceManagementOptions();
        var manifest = new ResourceManifest();
        manifest.DefineStyle("my-dep").SetUrl("dep.css");
        options.ResourceManifests.Add(manifest);

        var resourceManager = CreateResourceManager(options);
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "my-style"),
            ("src", "~/my-style.css"),
            ("depends_on", "my-dep"),
            ("at", "Inline")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Resource registered and rendered inline.
        var requiredResources = resourceManager.GetRequiredResources("stylesheet");
        Assert.Contains(requiredResources, ctx => ctx.Resource.Name == "my-style");

        var htmlBuilder = new HtmlContentBuilder();
        htmlBuilder.AppendHtml(writer.ToString());

        var document = await ParseHtmlAsync(htmlBuilder);
        var links = document.QuerySelectorAll<IHtmlLinkElement>("link").ToList();

        Assert.Equal(2, links.Count);
        Assert.Contains(links, l => l.Href.EndsWith("dep.css"));
        Assert.Contains(links, l => l.Href.EndsWith("my-style.css"));
    }

    [Fact]
    public async Task NamedStyleWithNameAndSrc_NoAt_DoesNotRegisterOrRender()
    {
        // Arrange — When at is Unspecified, the named+src path only defines, does not register/render.
        var resourceManager = CreateResourceManager();
        var context = CreateLiquidContext(resourceManager);
        var writer = new StringWriter();

        var arguments = CreateArguments(
            ("name", "my-style"),
            ("src", "~/my-style.css")
        );

        // Act
        await StyleTag.WriteToAsync(arguments, writer, null, context);

        // Assert — Defined in InlineManifest but NOT registered as required.
        var inlineDefinition = resourceManager.InlineManifest
            .GetResources("stylesheet")
            .FirstOrDefault(r => r.Key == "my-style")
            .Value?.FirstOrDefault();

        Assert.NotNull(inlineDefinition);

        var requiredResources = resourceManager.GetRequiredResources("stylesheet");
        Assert.Empty(requiredResources);
        Assert.Empty(writer.ToString());
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
        var context = CreateLiquidContext(resourceManager);

        var arguments = CreateArguments(
            ("src", "~/foo.css"),
            ("debug_src", "~/foo.debug.css"),
            ("depends_on", "my-dep"),
            ("at", "Head")
        );

        // Act — Register the same style twice.
        await StyleTag.WriteToAsync(arguments, new StringWriter(), null, context);
        await StyleTag.WriteToAsync(arguments, new StringWriter(), null, context);

        // Assert — Only one resource definition should exist.
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
