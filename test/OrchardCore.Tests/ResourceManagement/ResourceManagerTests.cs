using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using OrchardCore.ResourceManagement;
using ResourceLocation = OrchardCore.ResourceManagement.ResourceLocation;

namespace OrchardCore.Tests.ResourceManagement
{
    public class ResourceManagerTests
    {
        private const string BasePath = "http://host";

        private readonly IBrowsingContext _browsingContext;

        public ResourceManagerTests()
        {
            _browsingContext = BrowsingContext.New();
        }

        [Fact]
        public void FindResourceFromManifestProviders()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "bar1").SetAttribute("attr", "bar1");
            manifest.DefineResource("foo", "bar2").SetAttribute("attr", "bar2");
            manifest.DefineResource("foo", "bar3").SetAttribute("attr", "bar3");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            var resourceDefinition = resourceManager.FindResource(new RequireSettings { Type = "foo", Name = "bar2" });

            Assert.NotNull(resourceDefinition);
            Assert.Equal("foo", resourceDefinition.Type);
            Assert.Equal("bar2", resourceDefinition.Name);
            Assert.Contains("bar2", ((IDictionary<string, string>)resourceDefinition.Attributes).Values);
            Assert.Contains("attr", ((IDictionary<string, string>)resourceDefinition.Attributes).Keys);
        }

        [Fact]
        public void RegisterResouceUrl()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var requireSetting = resourceManager.RegisterUrl("foo", "schema://domain.ext/resource", "schema://domain.ext/resource.debug");

            var resourceDefinition = resourceManager.FindResource(requireSetting);
            Assert.NotNull(resourceDefinition);
            Assert.Equal("foo", resourceDefinition.Type);
            Assert.Equal("schema://domain.ext/resource", resourceDefinition.Url);
            Assert.Equal("schema://domain.ext/resource.debug", resourceDefinition.UrlDebug);
        }

        [Fact]
        public void RegisteredResouceUrlIsRequired()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterUrl("foo", "schema://domain.ext/resource", "schema://domain.ext/resource.debug");

            var requiredResourceContexts = resourceManager.GetRequiredResources("foo");
            var resourceDefinition = requiredResourceContexts.FirstOrDefault()?.Resource;

            Assert.NotNull(resourceDefinition);
            Assert.Equal("foo", resourceDefinition.Type);
            Assert.Equal("schema://domain.ext/resource", resourceDefinition.Url);
            Assert.Equal("schema://domain.ext/resource.debug", resourceDefinition.UrlDebug);
        }

        [Fact]
        public void RegisteredResouceNameIsRequired()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "bar").SetUrl("schema://domain.ext/resource", "schema://domain.ext/resource.debug");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "bar");

            var requiredResourceContexts = resourceManager.GetRequiredResources("foo");
            var resourceDefinition = requiredResourceContexts.FirstOrDefault()?.Resource;

            Assert.NotNull(resourceDefinition);
            Assert.Equal("foo", resourceDefinition.Type);
            Assert.Equal("schema://domain.ext/resource", resourceDefinition.Url);
            Assert.Equal("schema://domain.ext/resource.debug", resourceDefinition.UrlDebug);
        }

        [Fact]
        public void RequireDependencies()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "first-resource")
                .SetDependencies("first-dependency")
                .SetPosition(ResourcePosition.First);
            manifest.DefineResource("foo", "requires-dependency")
                .SetDependencies("dependency");
            manifest.DefineResource("foo", "dependency");
            manifest.DefineResource("foo", "another-dependency")
                .SetDependencies("first-dependency");
            manifest.DefineResource("foo", "first-dependency");
            manifest.DefineResource("foo", "last-dependency")
                .SetPosition(ResourcePosition.Last)
                .SetDependencies("another-dependency");
            manifest.DefineResource("foo", "simple-resource")
                .SetDependencies("first-dependency");
            manifest.DefineResource("foo", "last-resource")
                .SetPosition(ResourcePosition.Last)
                .SetDependencies("last-dependency");
            manifest.DefineResource("foo", "not-used-resource");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "last-resource");
            resourceManager.RegisterResource("foo", "requires-dependency");
            resourceManager.RegisterResource("foo", "first-resource");
            resourceManager.RegisterResource("foo", "simple-resource");

            var requiredResources = resourceManager.GetRequiredResources("foo")
                .Select(ctx => ctx.Resource)
                .ToList();

            // Ensure dependencies loaded
            Assert.True(requiredResources.Count == 8);

            // Ensure order
            var firstDependencyIndex = requiredResources.FindIndex(resource => resource.Name == "first-dependency");
            var firstResourceIndex = requiredResources.FindIndex(resource => resource.Name == "first-resource");
            var anotherDependencyIndex = requiredResources.FindIndex(resource => resource.Name == "another-dependency");
            var dependencyIndex = requiredResources.FindIndex(resource => resource.Name == "dependency");
            var requiresDependencyIndex = requiredResources.FindIndex(resource => resource.Name == "requires-dependency");
            var simpleResourceIndex = requiredResources.FindIndex(resource => resource.Name == "simple-resource");
            var lastDependecyIndex = requiredResources.FindIndex(resource => resource.Name == "last-dependency");
            var lastResourceIndex = requiredResources.FindIndex(resource => resource.Name == "last-resource");

            Assert.True(firstResourceIndex > firstDependencyIndex);
            Assert.True(anotherDependencyIndex > firstResourceIndex);
            Assert.True(dependencyIndex > anotherDependencyIndex);
            Assert.True(requiresDependencyIndex > dependencyIndex);
            Assert.True(simpleResourceIndex > requiresDependencyIndex);
            Assert.True(lastDependecyIndex > simpleResourceIndex);
            Assert.True(lastResourceIndex > lastDependecyIndex);
        }

        [Fact]
        public void RequireCircularDependenciesShouldThrowException()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "required")
                .SetDependencies("dependency");
            manifest.DefineResource("foo", "dependency")
                .SetDependencies("required");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "required");

            var ex = Assert.Throws<InvalidOperationException>(() => resourceManager.GetRequiredResources("foo"));
            Assert.StartsWith("Circular dependency", ex.Message);
        }

        [Fact]
        public void RequireCircularNestedDependencyShouldThrowException()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "requires-dependency")
                .SetDependencies("dependency");
            manifest.DefineResource("foo", "requires-indirect-dependency")
                .SetDependencies("requires-dependency");
            manifest.DefineResource("foo", "dependency")
                .SetDependencies("requires-indirect-dependency");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "requires-indirect-dependency");
            resourceManager.RegisterResource("foo", "requires-dependency");

            var ex = Assert.Throws<InvalidOperationException>(() => resourceManager.GetRequiredResources("foo"));
            Assert.StartsWith("Circular dependency", ex.Message);
        }

        [Fact]
        public void RequireByDependencyResourceThatDependsOnLastPositionedResourceShouldRegisterResourceLast()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "resource")
                .SetDependencies("last-resource");
            manifest.DefineResource("foo", "last-resource")
                .SetPosition(ResourcePosition.Last);

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "resource");

            var requiredResources = resourceManager.GetRequiredResources("foo")
                .Select(ctx => ctx.Resource)
                .ToList();

            // Ensure dependencies loaded
            Assert.True(requiredResources.Count == 2);

            // Ensure order
            var resourceIndex = requiredResources.FindIndex(resource => resource.Name == "resource");
            var lastResourceIndex = requiredResources.FindIndex(resource => resource.Name == "last-resource");

            Assert.True(resourceIndex > lastResourceIndex);
        }

        [Fact]
        public void RequireFirstPositionedResourceThatDependsOnByDependencyResourceShouldRegisterDependencyFirst()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "dependency");
            manifest.DefineResource("foo", "first-resource")
                .SetDependencies("dependency")
                .SetPosition(ResourcePosition.First);

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "first-resource");

            var requiredResources = resourceManager.GetRequiredResources("foo")
                .Select(ctx => ctx.Resource)
                .ToList();

            // Ensure dependencies loaded
            Assert.True(requiredResources.Count == 2);

            // Ensure order
            var dependencyIndex = requiredResources.FindIndex(resource => resource.Name == "dependency");
            var firstResourceIndex = requiredResources.FindIndex(resource => resource.Name == "first-resource");

            Assert.True(firstResourceIndex > dependencyIndex);
        }

        [Fact]
        public void RequireFirstPositionedResourceWithDependencyToResourcePositionedLastShouldThrowException()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "resource")
                .SetDependencies("last-resource");
            manifest.DefineResource("foo", "last-resource")
                .SetPosition(ResourcePosition.Last);
            manifest.DefineResource("foo", "first-resource")
                .SetPosition(ResourcePosition.First)
                .SetDependencies("resource");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "first-resource");
            resourceManager.RegisterResource("foo", "last-resource");

            var ex = Assert.Throws<InvalidOperationException>(() => resourceManager.GetRequiredResources("foo"));
            Assert.StartsWith("Invalid dependency position", ex.Message);
        }

        [Fact]
        public void RemoveRequiredResource()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "required");
            manifest.DefineResource("foo", "to-remove");
            manifest.DefineResource("foo", "not-required");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "required");
            resourceManager.RegisterResource("foo", "to-remove");

            resourceManager.NotRequired("foo", "to-remove");

            var requiredResources = resourceManager.GetRequiredResources("foo")
                .Select(ctx => ctx.Resource)
                .ToList();

            Assert.Contains(requiredResources, resource => resource.Name == "required");
            Assert.DoesNotContain(requiredResources, resource => resource.Name == "to-remove");
            Assert.DoesNotContain(requiredResources, resource => resource.Name == "not-required");
        }

        [Fact]
        public void RemoveRequiredResourceDependency()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineResource("foo", "required");
            manifest.DefineResource("foo", "to-remove")
                .SetDependencies("dependency");
            manifest.DefineResource("foo", "dependency");
            manifest.DefineResource("foo", "not-required");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "required");
            resourceManager.RegisterResource("foo", "to-remove");

            resourceManager.NotRequired("foo", "to-remove");

            var requiredResources = resourceManager.GetRequiredResources("foo")
                .Select(ctx => ctx.Resource)
                .ToList();

            Assert.Contains(requiredResources, resource => resource.Name == "required");
            Assert.DoesNotContain(requiredResources, resource => resource.Name == "to-remove");
            Assert.DoesNotContain(requiredResources, resource => resource.Name == "dependency");
            Assert.DoesNotContain(requiredResources, resource => resource.Name == "not-required");
        }

        [Fact]
        public void RegisterHeadScript()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var customScript = "<script>doSomeAction();</script>";
            resourceManager.RegisterHeadScript(new HtmlString(customScript));

            var registeredScripts = resourceManager.GetRegisteredHeadScripts();

            Assert.Contains(registeredScripts, script => script.ToString() == customScript);
        }

        [Fact]
        public void RegisterFootScript()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var customScript = "<script>doSomeAction();</script>";
            resourceManager.RegisterFootScript(new HtmlString(customScript));

            var registeredScripts = resourceManager.GetRegisteredFootScripts();

            Assert.Contains(registeredScripts, script => script.ToString() == customScript);
        }

        [Fact]
        public void RegisterStyle()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var customStyle = "<style>.my-class { prop: value; }</style>";
            resourceManager.RegisterStyle(new HtmlString(customStyle));

            var registeredStyles = resourceManager.GetRegisteredStyles();

            Assert.Contains(registeredStyles, style => style.ToString() == customStyle);
        }

        [Fact]
        public void RegisterLink()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var linkEntry = new LinkEntry
            {
                Rel = "foo",
                Href = "bar.ext"
            };

            resourceManager.RegisterLink(linkEntry);

            var registeredLinks = resourceManager.GetRegisteredLinks();

            Assert.Contains(linkEntry, registeredLinks);
        }

        [Fact]
        public void RegisterMeta()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var metaEntry = new MetaEntry(name: "foo", content: "bar");

            resourceManager.RegisterMeta(metaEntry);

            var registeredMetas = resourceManager.GetRegisteredMetas();

            Assert.Contains(metaEntry, registeredMetas);
        }

        [Fact]
        public async Task AppendMeta()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var first = new MetaEntry(name: "keywords", content: "bar")
                .AddAttribute("attr1", "val1");
            var second = new MetaEntry(name: "keywords", content: "baz")
                .AddAttribute("attr2", "val2");

            resourceManager.AppendMeta(first, ",");
            resourceManager.AppendMeta(second, ",");

            var keywordsMeta = resourceManager.GetRegisteredMetas()
                .FirstOrDefault(entry => entry.Name == "keywords");

            var document = await ParseHtmlAsync(keywordsMeta.GetTag());
            var meta = document
                .QuerySelector<IHtmlMetaElement>("meta[name=keywords]");

            Assert.Equal("bar,baz", meta.Content);
            Assert.Contains(meta.Attributes, attr => attr.Name == "attr1" && attr.Value == "val1");
            Assert.Contains(meta.Attributes, attr => attr.Name == "attr2" && attr.Value == "val2");
        }

        [Fact]
        public async Task RenderMeta()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterMeta(new MetaEntry { Charset = "utf-8" });
            resourceManager.RegisterMeta(new MetaEntry { Name = "description", Content = "Some content" });
            resourceManager.RegisterMeta(new MetaEntry { HttpEquiv = "refresh", Content = "3;url=https://www.orchardcore.net/" });

            using var sw = new StringWriter();
            resourceManager.RenderMeta(sw);
            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw.ToString());

            var document = await ParseHtmlAsync(htmlBuilder);
            var metas = document
                .QuerySelectorAll<IHtmlMetaElement>("meta");

            Assert.Equal(3, metas.Count());
            Assert.Contains(metas, meta => meta.Charset == "utf-8");
            Assert.Contains(metas, meta => meta.Name == "description" && meta.Content == "Some content");
            Assert.Contains(metas, meta => meta.HttpEquivalent == "refresh" && meta.Content == "3;url=https://www.orchardcore.net/");
        }

        [Fact]
        public async Task RenderHeadLink()
        {
            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterUrl("stylesheet", "other.min.css", "other.css");    // Should not be rendered
            resourceManager.RegisterLink(new LinkEntry { Rel = "icon", Href = "/favicon.ico" });
            resourceManager.RegisterLink(new LinkEntry { Rel = "alternate", Type = "application/pdf", Href = "/pdf" });

            using var sw = new StringWriter();
            resourceManager.RenderHeadLink(sw);
            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw.ToString());

            var document = await ParseHtmlAsync(htmlBuilder);
            var links = document
                .QuerySelectorAll<IHtmlLinkElement>("link");

            Assert.Equal(2, links.Count());
            Assert.Contains(links, link => link.Relation == "icon" && link.Href == $"{BasePath}/favicon.ico");
            Assert.Contains(links, link => link.Relation == "alternate" && link.Type == "application/pdf" && link.Href == $"{BasePath}/pdf");
        }

        [Fact]
        public async Task RenderStylesheet()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineStyle("required").SetUrl("required.css")
                .SetDependencies("dependency");
            manifest.DefineStyle("dependency").SetUrl("dependency.css");
            manifest.DefineStyle("not-required").SetUrl("not-required.css");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterLink(new LinkEntry { Rel = "icon", Href = "/favicon.ico" });    // Should not be rendered

            // Require resource
            resourceManager.RegisterResource("stylesheet", "required");

            // Register custom style
            var customStyle = ".my-class { prop: value; }";
            resourceManager.RegisterStyle(new HtmlString($"<style>{customStyle}</style>"));

            using var sw = new StringWriter();
            resourceManager.RenderStylesheet(sw);
            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw.ToString());

            var document = await ParseHtmlAsync(htmlBuilder);
            var links = document
                .QuerySelectorAll<IHtmlLinkElement>("link");
            var styles = document
                .QuerySelectorAll<IHtmlStyleElement>("style");

            Assert.Equal(2, links.Count());
            Assert.Contains(links, link => link.Href == $"{BasePath}/dependency.css");
            Assert.Contains(links, link => link.Href == $"{BasePath}/required.css");
            Assert.Single(styles);
            Assert.Contains(styles, style => style.InnerHtml == customStyle);
            // Required stylesheet after its dependency
            Assert.Equal(DocumentPositions.Following, links.First(link => link.Href == $"{BasePath}/dependency.css")
                .CompareDocumentPosition(
                    links.First(link => link.Href == $"{BasePath}/required.css")
                )
            );
            // Custom style after resources
            Assert.Equal(DocumentPositions.Following, links.First(link => link.Href == $"{BasePath}/required.css")
                .CompareDocumentPosition(
                    styles.First(style => style.InnerHtml == customStyle)
                )
            );
        }

        [Fact]
        public async Task RenderHeadScript()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineScript("required").SetUrl("required.js")
                .SetDependencies("dependency");
            manifest.DefineScript("dependency").SetUrl("dependency.js");
            manifest.DefineScript("not-required").SetUrl("not-required.js");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            // Require resource
            resourceManager.RegisterResource("script", "required").AtHead();

            // Register custom script
            var customScript = "doSomeAction();";
            resourceManager.RegisterHeadScript(new HtmlString($"<script>{customScript}</script>"));

            using var sw = new StringWriter();
            resourceManager.RenderHeadScript(sw);
            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw.ToString());

            var document = await ParseHtmlAsync(htmlBuilder);
            var scripts = document
                .QuerySelectorAll<IHtmlScriptElement>("script");

            Assert.Equal(3, scripts.Count());
            Assert.Contains(scripts, script => script.Source == "dependency.js");
            Assert.Contains(scripts, script => script.Source == "required.js");
            Assert.Contains(scripts, script => script.Text == customScript);
            // Required script after its dependency
            Assert.Equal(DocumentPositions.Following, scripts.First(script => script.Source == "dependency.js")
                .CompareDocumentPosition(
                    scripts.First(script => script.Source == "required.js")
                )
            );
            // Custom script after resources
            Assert.Equal(DocumentPositions.Following, scripts.First(script => script.Source == "required.js")
                .CompareDocumentPosition(
                    scripts.First(script => script.Text == customScript)
                )
            );
        }

        [Fact]
        public async Task RenderFootScript()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineScript("required").SetUrl("required.js")
                .SetDependencies("dependency");
            manifest.DefineScript("dependency").SetUrl("dependency.js");
            manifest.DefineScript("not-required").SetUrl("not-required.js");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            // Require resource
            resourceManager.RegisterResource("script", "required").AtFoot();

            // Register custom script
            var customScript = "doSomeAction();";
            resourceManager.RegisterFootScript(new HtmlString($"<script>{customScript}</script>"));

            using var sw = new StringWriter();
            resourceManager.RenderFootScript(sw);
            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw.ToString());

            var document = await ParseHtmlAsync(htmlBuilder);
            var scripts = document
                .QuerySelectorAll<IHtmlScriptElement>("script");

            Assert.Equal(3, scripts.Count());
            Assert.Contains(scripts, script => script.Source == "dependency.js");
            Assert.Contains(scripts, script => script.Source == "required.js");
            Assert.Contains(scripts, script => script.Text == customScript);
            // Required script after its dependency
            Assert.Equal(DocumentPositions.Following, scripts.First(script => script.Source == "dependency.js")
                .CompareDocumentPosition(
                    scripts.First(script => script.Source == "required.js")
                )
            );
            // Custom script after resources
            Assert.Equal(DocumentPositions.Following, scripts.First(script => script.Source == "required.js")
                .CompareDocumentPosition(
                    scripts.First(script => script.Text == customScript)
                )
            );
        }

        [Fact]
        public async Task RenderHeadAndFootScriptWithSameDependency()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineScript("required")
                .SetUrl("required.js")
                .SetDependencies("dependency");

            manifest.DefineScript("dependency")
                .SetUrl("dependency.js");

            manifest.DefineScript("required-at-foot")
                .SetUrl("required-at-foot.js")
                .SetDependencies("dependency");

            manifest.DefineScript("not-required")
                .SetUrl("not-required.js");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager
            (
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            // Require resource.
            resourceManager.RegisterResource("script", "required").AtHead();

            // Register custom script.
            var customScript = "doSomeAction();";
            resourceManager.RegisterHeadScript(new HtmlString($"<script>{customScript}</script>"));

            // Require resource at Foot with same dependency at Head
            resourceManager.RegisterResource("script", "required-at-foot").AtFoot();

            using var sw1 = new StringWriter();
            resourceManager.RenderHeadScript(sw1);
            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw1.ToString());

            var document = await ParseHtmlAsync(htmlBuilder);
            var headScripts = document
                .QuerySelectorAll<IHtmlScriptElement>("script");

            using var sw2 = new StringWriter();
            resourceManager.RenderFootScript(sw2);
            htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw2.ToString());

            document = await ParseHtmlAsync(htmlBuilder);
            var footScripts = document
                .QuerySelectorAll<IHtmlScriptElement>("script");

            // Should render 4 scripts in total, 3 at head and 1 at foot.
            Assert.Equal(4, headScripts.Count() + footScripts.Count());

            //Check head script positions.
            Assert.Contains("dependency.js", headScripts.ElementAt(0).Source);
            Assert.Contains("required.js", headScripts.ElementAt(1).Source);
            Assert.Contains(customScript, headScripts.ElementAt(2).Text);

            //Check foot script positions.
            Assert.Contains("required-at-foot.js", footScripts.ElementAt(0).Source);
        }

        [Fact]
        public async Task RenderLocalScript()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineScript("required").SetUrl("required.js")
                .SetDependencies("dependency");
            manifest.DefineScript("dependency").SetUrl("dependency.js");
            manifest.DefineScript("not-required").SetUrl("not-required.js");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            var requireSetting = resourceManager.RegisterResource("script", "required");

            using var sw = new StringWriter();
            resourceManager.RenderLocalScript(requireSetting, sw);
            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw.ToString());


            var document = await ParseHtmlAsync(htmlBuilder);
            var scripts = document
                .QuerySelectorAll<IHtmlScriptElement>("script");

            Assert.Equal(2, scripts.Count());
            Assert.Contains(scripts, script => script.Source == "dependency.js");
            Assert.Contains(scripts, script => script.Source == "required.js");
            Assert.Equal(DocumentPositions.Following, scripts.First(script => script.Source == "dependency.js")
                .CompareDocumentPosition(
                    scripts.First(script => script.Source == "required.js")
                )
            );
        }

        [Fact]
        public async Task RenderLocalStyle()
        {
            var options = new ResourceManagementOptions();
            var manifest = new ResourceManifest();

            manifest.DefineStyle("required").SetUrl("required.css")
                .SetDependencies("dependency");
            manifest.DefineStyle("dependency").SetUrl("dependency.css");
            manifest.DefineStyle("not-required").SetUrl("not-required.css");

            options.ResourceManifests.Add(manifest);

            var resourceManager = new ResourceManager(
                new OptionsWrapper<ResourceManagementOptions>(options),
                StubFileVersionProvider.Instance
            );

            var requireSetting = resourceManager.RegisterResource("stylesheet", "required").AtLocation(ResourceLocation.Inline);

            using var sw = new StringWriter();
            resourceManager.RenderLocalStyle(requireSetting, sw);
            var htmlBuilder = new HtmlContentBuilder();
            htmlBuilder.AppendHtml(sw.ToString());

            var document = await ParseHtmlAsync(htmlBuilder);
            var scripts = document
                .QuerySelectorAll<IHtmlLinkElement>("link");

            Assert.Equal(2, scripts.Count());
            Assert.Contains(scripts, script => script.Href.EndsWith("dependency.css"));
            Assert.Contains(scripts, script => script.Href.EndsWith("required.css"));
            Assert.Equal(DocumentPositions.Following, scripts.First(script => script.Href.EndsWith("dependency.css"))
                .CompareDocumentPosition(
                    scripts.First(script => script.Href.EndsWith("required.css"))
                )
            );
        }

        #region Helpers
        private async Task<IDocument> ParseHtmlAsync(IHtmlContent content)
        {
            using var writer = new StringWriter();

            content.WriteTo(writer, HtmlEncoder.Default);

            return await _browsingContext.OpenAsync(res => res.Content(writer.ToString()).Address(BasePath));
        }

        #endregion

        #region Stubs
        private class StubResourceManifestProvider : IConfigureOptions<ResourceManagementOptions>
        {
            private readonly Action<ResourceManagementOptions> _configureManifestAction;

            public StubResourceManifestProvider(Action<ResourceManagementOptions> configureManifestAction)
            {
                _configureManifestAction = configureManifestAction;
            }

            public void Configure(ResourceManagementOptions options)
            {
                _configureManifestAction?.Invoke(options);
            }
        }

        private class StubFileVersionProvider : IFileVersionProvider
        {
            public static StubFileVersionProvider Instance { get; } = new StubFileVersionProvider();

            public string AddFileVersionToPath(PathString requestPathBase, string path)
            {
                return path;
            }
        }

        #endregion
    }
}
