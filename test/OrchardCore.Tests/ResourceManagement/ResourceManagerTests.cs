using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using Xunit;

namespace OrchardCore.Tests.ResourceManagement
{
    public class ResourceManagerTests
    {
        private const string basePath = "http://host";

        private readonly IBrowsingContext browsingContext;

        public ResourceManagerTests()
        {
            browsingContext = BrowsingContext.New();
        }

        [Fact]
        public void FindResourceFromManifestProviders()
        {
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineResource("foo", "bar1").SetAttribute("attr", "bar1");
                        manifest.DefineResource("foo", "bar2").SetAttribute("attr", "bar2");
                        manifest.DefineResource("foo", "bar3").SetAttribute("attr", "bar3");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var resourceDefinition = resourceManager.FindResource(new RequireSettings { Type = "foo", Name = "bar2" });

            Assert.NotNull(resourceDefinition);
            Assert.Equal("foo", resourceDefinition.Type);
            Assert.Equal("bar2", resourceDefinition.Name);
            Assert.Equal("bar2", Assert.Contains("attr", (IDictionary<string, string>)resourceDefinition.Attributes));
        }

        [Fact]
        public void RegisterResouceUrl()
        {
            var resourceManager = new ResourceManager(
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
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
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineResource("foo", "bar")
                            .SetUrl("schema://domain.ext/resource", "schema://domain.ext/resource.debug");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
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
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineResource("foo", "required")
                            .SetDependencies("dependency");
                        manifest.DefineResource("foo", "dependency");
                        manifest.DefineResource("foo", "not-required");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterResource("foo", "required");

            var requiredResources = resourceManager.GetRequiredResources("foo")
                .Select(ctx => ctx.Resource)
                .ToList();

            Assert.Contains(requiredResources, resource => resource.Name == "required");
            Assert.Contains(requiredResources, resource => resource.Name == "dependency");
            Assert.DoesNotContain(requiredResources, resource => resource.Name == "not-required");

            // Ensure order
            var requiredIndex = requiredResources.FindIndex(resource => resource.Name == "required");
            var dependecyIndex = requiredResources.FindIndex(resource => resource.Name == "dependency");
            Assert.True(requiredIndex > dependecyIndex);
        }

        [Fact]
        public void RemoveRequiredResource()
        {
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineResource("foo", "required");
                        manifest.DefineResource("foo", "to-remove");
                        manifest.DefineResource("foo", "not-required");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
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
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineResource("foo", "required");
                        manifest.DefineResource("foo", "to-remove")
                            .SetDependencies("dependency");
                        manifest.DefineResource("foo", "dependency");
                        manifest.DefineResource("foo", "not-required");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var linkEntry = new LinkEntry {
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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterMeta(new MetaEntry { Charset = "utf-8" });
            resourceManager.RegisterMeta(new MetaEntry { Name = "description", Content = "Some content" });
            resourceManager.RegisterMeta(new MetaEntry { HttpEquiv = "refresh", Content = "3;url=https://www.orchardcore.net/" });

            var htmlBuilder = new HtmlContentBuilder();
            resourceManager.RenderMeta(htmlBuilder);

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
                Enumerable.Empty<IResourceManifestProvider>(),
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterUrl("stylesheet", "other.min.css", "other.css");    // Should not be rendered
            resourceManager.RegisterLink(new LinkEntry { Rel = "icon", Href = "/favicon.ico" });
            resourceManager.RegisterLink(new LinkEntry { Rel = "alternate", Type = "application/pdf", Href = "/pdf" });

            var htmlBuilder = new HtmlContentBuilder();
            resourceManager.RenderHeadLink(htmlBuilder);

            var document = await ParseHtmlAsync(htmlBuilder);
            var links = document
                .QuerySelectorAll<IHtmlLinkElement>("link");

            Assert.Equal(2, links.Count());
            Assert.Contains(links, link => link.Relation == "icon" && link.Href == $"{basePath}/favicon.ico");
            Assert.Contains(links, link => link.Relation == "alternate" && link.Type == "application/pdf" && link.Href == $"{basePath}/pdf");
        }

        [Fact]
        public async Task RenderStylesheet()
        {
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineStyle("required").SetUrl("required.css")
                            .SetDependencies("dependency");
                        manifest.DefineStyle("dependency").SetUrl("dependency.css");
                        manifest.DefineStyle("not-required").SetUrl("not-required.css");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            resourceManager.RegisterLink(new LinkEntry { Rel = "icon", Href = "/favicon.ico" });    // Should not be rendered

            // Require resource
            resourceManager.RegisterResource("stylesheet", "required");

            // Register custom style
            var customStyle = ".my-class { prop: value; }";
            resourceManager.RegisterStyle(new HtmlString($"<style>{customStyle}</style>"));

            var htmlBuilder = new HtmlContentBuilder();
            resourceManager.RenderStylesheet(htmlBuilder);

            var document = await ParseHtmlAsync(htmlBuilder);
            var links = document
                .QuerySelectorAll<IHtmlLinkElement>("link");
            var styles = document
                .QuerySelectorAll<IHtmlStyleElement>("style");

            Assert.Equal(2, links.Count());
            Assert.Contains(links, link => link.Href == $"{basePath}/dependency.css");
            Assert.Contains(links, link => link.Href == $"{basePath}/required.css");
            Assert.Single(styles);
            Assert.Contains(styles, style => style.InnerHtml == customStyle);
            // Required stylesheet after its dependency
            Assert.Equal(DocumentPositions.Following, links.First(link => link.Href == $"{basePath}/dependency.css")
                .CompareDocumentPosition(
                    links.First(link => link.Href == $"{basePath}/required.css")
                )
            );
            // Custom style after resources
            Assert.Equal(DocumentPositions.Following, links.First(link => link.Href == $"{basePath}/required.css")
                .CompareDocumentPosition(
                    styles.First(style => style.InnerHtml == customStyle)
                )
            );
        }

        [Fact]
        public async Task RenderHeadScript()
        {
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineScript("required").SetUrl("required.js")
                            .SetDependencies("dependency");
                        manifest.DefineScript("dependency").SetUrl("dependency.js");
                        manifest.DefineScript("not-required").SetUrl("not-required.js");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            // Require resource
            resourceManager.RegisterResource("script", "required").AtHead();

            // Register custom script
            var customScript = "doSomeAction();";
            resourceManager.RegisterHeadScript(new HtmlString($"<script>{customScript}</script>"));

            var htmlBuilder = new HtmlContentBuilder();
            resourceManager.RenderHeadScript(htmlBuilder);

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
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineScript("required").SetUrl("required.js")
                            .SetDependencies("dependency");
                        manifest.DefineScript("dependency").SetUrl("dependency.js");
                        manifest.DefineScript("not-required").SetUrl("not-required.js");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            // Require resource
            resourceManager.RegisterResource("script", "required").AtFoot();

            // Register custom script
            var customScript = "doSomeAction();";
            resourceManager.RegisterFootScript(new HtmlString($"<script>{customScript}</script>"));

            var htmlBuilder = new HtmlContentBuilder();
            resourceManager.RenderFootScript(htmlBuilder);

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
        public async Task RenderLocalScript()
        {
            var resourceManager = new ResourceManager(
                new[] {
                    new StubResourceManifestProvider(builder => {
                        var manifest = builder.Add();
                        manifest.DefineScript("required").SetUrl("required.js")
                            .SetDependencies("dependency");
                        manifest.DefineScript("dependency").SetUrl("dependency.js");
                        manifest.DefineScript("not-required").SetUrl("not-required.js");
                    })
                },
                new ResourceManifestState(),
                new OptionsWrapper<ResourceManagementOptions>(new ResourceManagementOptions()),
                StubFileVersionProvider.Instance
            );

            var requireSetting = resourceManager.RegisterResource("script", "required");

            var htmlBuilder = new HtmlContentBuilder();
            resourceManager.RenderLocalScript(requireSetting, htmlBuilder);

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

        #region Helpers
        private async Task<IDocument> ParseHtmlAsync(IHtmlContent content)
        {
            using (var writer = new StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);

                return await browsingContext.OpenAsync(res => res.Content(writer.ToString()).Address(basePath));
            }
        }

        #endregion

        #region Stubs
        private class StubResourceManifestProvider : IResourceManifestProvider
        {
            private readonly Action<IResourceManifestBuilder> _configureManifestAction;

            public StubResourceManifestProvider(Action<IResourceManifestBuilder> configureManifestAction)
            {
                _configureManifestAction = configureManifestAction;
            }

            public void BuildManifests(IResourceManifestBuilder builder)
            {
                _configureManifestAction?.Invoke(builder);
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
