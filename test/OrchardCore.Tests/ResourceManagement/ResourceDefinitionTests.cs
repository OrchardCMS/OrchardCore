using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OrchardCore.ResourceManagement;
using Xunit;

namespace OrchardCore.Tests.ResourceManagement
{
    public class ResourceDefinitionTests
    {
        private const string basePath = "http://host";
        private readonly ResourceManifest _resourceManifest;

        public ResourceDefinitionTests()
        {
            _resourceManifest = new ResourceManifest();
        }

        [Fact]
        public void GetScriptResourceWithUrl()
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetCdn("https://cdn.tld/foo.js", "https://cdn.tld/foo.debug.js");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal("text/javascript", tagBuilder.Attributes["type"]);
            Assert.Equal($"{basePath}/foo.js", tagBuilder.Attributes["src"]);
        }

        [Fact]
        public void GetScriptResourceWithDebugUrl()
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetCdn("https://cdn.tld/foo.js", "https://cdn.tld/foo.debug.js");

            var requireSettings = new RequireSettings { DebugMode = true, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal("text/javascript", tagBuilder.Attributes["type"]);
            Assert.Equal($"{basePath}/foo.debug.js", tagBuilder.Attributes["src"]);
        }

        [Fact]
        public void GetScriptResourceWithCdnUrl()
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetCdn("https://cdn.tld/foo.js", "https://cdn.tld/foo.debug.js");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = true };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal("text/javascript", tagBuilder.Attributes["type"]);
            Assert.Equal("https://cdn.tld/foo.js", tagBuilder.Attributes["src"]);
        }

        [Fact]
        public void GetScriptResourceWithDebugCdnUrl()
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetCdn("https://cdn.tld/foo.js", "https://cdn.tld/foo.debug.js");

            var requireSettings = new RequireSettings { DebugMode = true, CdnMode = true };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal("text/javascript", tagBuilder.Attributes["type"]);
            Assert.Equal("https://cdn.tld/foo.debug.js", tagBuilder.Attributes["src"]);
        }

        [Fact]
        public void GetScriptResourceWithInlineContent()
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetInnerContent("console.log('foo');");

            var requireSettings = new RequireSettings();
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal("text/javascript", tagBuilder.Attributes["type"]);
            Assert.Equal("console.log('foo');", ReadIHtmlContent(tagBuilder.InnerHtml));
        }

        [Fact]
        public void GetStyleResourceWithUrl()
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetCdn("https://cdn.tld/foo.css", "https://cdn.tld/foo.debug.css");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal($"{basePath}/foo.css", tagBuilder.Attributes["href"]);
        }

        [Fact]
        public void GetStyleResourceWithDebugUrl()
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetCdn("https://cdn.tld/foo.css", "https://cdn.tld/foo.debug.css");

            var requireSettings = new RequireSettings { DebugMode = true, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal($"{basePath}/foo.debug.css", tagBuilder.Attributes["href"]);
        }

        [Fact]
        public void GetStyleResourceWithCdnUrl()
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetCdn("https://cdn.tld/foo.css", "https://cdn.tld/foo.debug.css");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = true };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal("https://cdn.tld/foo.css", tagBuilder.Attributes["href"]);
        }

        [Fact]
        public void GetStyleResourceWithDebugCdnUrl()
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetCdn("https://cdn.tld/foo.css", "https://cdn.tld/foo.debug.css");

            var requireSettings = new RequireSettings { DebugMode = true, CdnMode = true };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal("https://cdn.tld/foo.debug.css", tagBuilder.Attributes["href"]);
        }

        [Fact]
        public void GetStyleResourceWithInlineContent()
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetInnerContent("body { background-color: white; }");

            var requireSettings = new RequireSettings();
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, basePath, StubFileVersionProvider.Instance);

            Assert.Equal("style", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("body { background-color: white; }", ReadIHtmlContent(tagBuilder.InnerHtml));
        }

        #region Helpers
        private static string ReadIHtmlContent(IHtmlContent content)
        {
            using (var writer = new StringWriter())
            {
                content?.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        #endregion

        #region Stubs
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
