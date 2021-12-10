using System;
using System.IO;
using System.Linq;
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
        private readonly ResourceManifest _resourceManifest;

        public ResourceDefinitionTests()
        {
            _resourceManifest = new ResourceManifest();
        }

        // The application path is empty if it is the host site
        // or contains the base path of a tenant.
        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetScriptResourceWithUrl(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetCdn("https://cdn.tld/foo.js", "https://cdn.tld/foo.debug.js");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal($"{applicationPath}/foo.js", tagBuilder.Attributes["src"]);
        }

        [Theory]
        [InlineData("/base")]
        public void GetScriptResourceWithBasePath(string basePath)
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetBasePath(basePath);

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, String.Empty, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal($"{basePath}/foo.js", tagBuilder.Attributes["src"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetScriptResourceWithDebugUrl(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetCdn("https://cdn.tld/foo.js", "https://cdn.tld/foo.debug.js");

            var requireSettings = new RequireSettings { DebugMode = true, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal($"{applicationPath}/foo.debug.js", tagBuilder.Attributes["src"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetScriptResourceWithCdnUrl(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetCdn("https://cdn.tld/foo.js", "https://cdn.tld/foo.debug.js");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = true };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal("https://cdn.tld/foo.js", tagBuilder.Attributes["src"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetScriptResourceWithDebugCdnUrl(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl("~/foo.js", "~/foo.debug.js")
                .SetCdn("https://cdn.tld/foo.js", "https://cdn.tld/foo.debug.js");

            var requireSettings = new RequireSettings { DebugMode = true, CdnMode = true, CdnBaseUrl = "https://hostcdn.net" };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal("https://cdn.tld/foo.debug.js", tagBuilder.Attributes["src"]);
        }

        [Theory]
        [InlineData("", "~/foo.js", "https://hostcdn.net/foo.js")]
        [InlineData("/tenant", "~/foo.js", "https://hostcdn.net/tenant/foo.js")]
        [InlineData("", "//external.com/foo.js", "//external.com/foo.js")]
        [InlineData("/tenant", "//external.com/foo.js", "//external.com/foo.js")]
        [InlineData("", "http://external.com/foo.js", "http://external.com/foo.js")]
        [InlineData("/tenant", "http://external.com/foo.js", "http://external.com/foo.js")]
        [InlineData("", "https://external.com/foo.js", "https://external.com/foo.js")]
        [InlineData("/tenant", "https://external.com/foo.js", "https://external.com/foo.js")]
        public void GetLocalScriptResourceWithCdnBaseUrl(string applicationPath, string url, string expected)
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetUrl(url, url);

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = true, CdnBaseUrl = "https://hostcdn.net" };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal(expected, tagBuilder.Attributes["src"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetScriptResourceWithInlineContent(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineScript("foo")
                .SetInnerContent("console.log('foo');");

            var requireSettings = new RequireSettings()
                .UseCdnBaseUrl("https://hostcdn.net");
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("script", tagBuilder.TagName);
            Assert.Equal("console.log('foo');", ReadIHtmlContent(tagBuilder.InnerHtml));
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetStyleResourceWithUrl(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetCdn("https://cdn.tld/foo.css", "https://cdn.tld/foo.debug.css");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal($"{applicationPath}/foo.css", tagBuilder.Attributes["href"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetStyleResourceWithDebugUrl(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetCdn("https://cdn.tld/foo.css", "https://cdn.tld/foo.debug.css");

            var requireSettings = new RequireSettings { DebugMode = true, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal($"{applicationPath}/foo.debug.css", tagBuilder.Attributes["href"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetStyleResourceWithCdnUrl(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetCdn("https://cdn.tld/foo.css", "https://cdn.tld/foo.debug.css");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = true };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal("https://cdn.tld/foo.css", tagBuilder.Attributes["href"]);
        }

        [Theory]
        [InlineData("", "~/foo.css", "https://hostcdn.net/foo.css")]
        [InlineData("/tenant", "~/foo.css", "https://hostcdn.net/tenant/foo.css")]
        [InlineData("", "//external.com/foo.css", "//external.com/foo.css")]
        [InlineData("/tenant", "//external.com/foo.css", "//external.com/foo.css")]
        [InlineData("", "http://external.com/foo.css", "http://external.com/foo.css")]
        [InlineData("/tenant", "http://external.com/foo.css", "http://external.com/foo.css")]
        [InlineData("", "https://external.com/foo.css", "https://external.com/foo.css")]
        [InlineData("/tenant", "https://external.com/foo.css", "https://external.com/foo.css")]
        public void GetLocalStyleResourceWithCdnBaseUrl(string applicationPath, string url, string expected)
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl(url, url);

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = true, CdnBaseUrl = "https://hostcdn.net" };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal(expected, tagBuilder.Attributes["href"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetStyleResourceWithDebugCdnUrl(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetCdn("https://cdn.tld/foo.css", "https://cdn.tld/foo.debug.css");

            var requireSettings = new RequireSettings { DebugMode = true, CdnMode = true };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal("https://cdn.tld/foo.debug.css", tagBuilder.Attributes["href"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetStyleResourceWithAttributes(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetUrl("~/foo.css", "~/foo.debug.css")
                .SetAttribute("id", "foo")
                .SetAttribute("media", "all");

            var requireSettings = new RequireSettings { DebugMode = false, CdnMode = false };
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("link", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("stylesheet", tagBuilder.Attributes["rel"]);
            Assert.Equal("foo", tagBuilder.Attributes["id"]);
            Assert.Equal("all", tagBuilder.Attributes["media"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("/tenant")]
        [InlineData("/virtualpath/tenant")]
        public void GetStyleResourceWithInlineContent(string applicationPath)
        {
            var resourceDefinition = _resourceManifest.DefineStyle("foo")
                .SetInnerContent("body { background-color: white; }");

            var requireSettings = new RequireSettings()
                .UseCdnBaseUrl("https://cdn.net");
            var tagBuilder = resourceDefinition.GetTagBuilder(requireSettings, applicationPath, StubFileVersionProvider.Instance);

            Assert.Equal("style", tagBuilder.TagName);
            Assert.Equal("text/css", tagBuilder.Attributes["type"]);
            Assert.Equal("body { background-color: white; }", ReadIHtmlContent(tagBuilder.InnerHtml));
        }

        [Fact]
        public void AbilityToSetUrlOnly()
        {
            // Arrange
            var url = "~/OrchardCore.Resources/Scripts/test.js";

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetUrl(url);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.NotNull(resource.Url);
            Assert.Null(resource.UrlDebug);
        }

        [Fact]
        public void AbilityToSetDebugUrlOnly()
        {
            // Arrange
            var url = "~/OrchardCore.Resources/Scripts/test.min.js";

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetUrl(url, debug: true);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.Null(resource.Url);
            Assert.NotNull(resource.UrlDebug);
        }

        [Fact]
        public void AbilityToSetBothUrlAndDebugUrl()
        {
            // Arrange
            var url = "~/OrchardCore.Resources/Scripts/test.js";
            var debugUrl = "~/OrchardCore.Resources/Scripts/test.min.js";

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetUrl(url, debugUrl);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.NotNull(resource.Url);
            Assert.NotNull(resource.UrlDebug);
        }

        [Theory]
        [InlineData("http://www.domain.com/test.js", false)]
        [InlineData("https://www.domain.com/test.js", true)]
        public void AbilityToSetCdnUrlOnly(string cdnUrl, bool supportsSSL)
        {
            // Arrange

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetCdn(cdnUrl, supportsSsl: supportsSSL);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.NotNull(resource.UrlCdn);
            Assert.Null(resource.UrlCdnDebug);
            Assert.Equal(supportsSSL, resource.CdnSupportsSsl);
        }

        [Theory]
        [InlineData("http://www.domain.com/test.min.js", false)]
        [InlineData("https://www.domain.com/test.min.js", true)]
        public void AbilityToSetCdnDebugUrlOnly(string cdnUrl, bool supportsSSL)
        {
            // Arrange

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetCdn(cdnUrl, debug: true, supportsSsl: supportsSSL);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.Null(resource.UrlCdn);
            Assert.NotNull(resource.UrlCdnDebug);
            Assert.Equal(supportsSSL, resource.CdnSupportsSsl);
        }

        [Theory]
        [InlineData("http://www.domain.com/test.js", "http://www.domain.com/test.min.js", false)]
        [InlineData("https://www.domain.com/test.js", "https://www.domain.com/test.min.js", true)]
        public void AbilityToSetBothCdnUrlAndCdnDebugUrl(string cdnUrl, string cdnDebugUrl, bool supportsSSL)
        {
            // Arrange

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetCdn(cdnUrl, cdnDebugUrl, supportsSsl: supportsSSL);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.NotNull(resource.UrlCdn);
            Assert.NotNull(resource.UrlCdnDebug);
            Assert.Equal(supportsSSL, resource.CdnSupportsSsl);
        }

        [Fact]
        public void AbilityToSetCdnIntegrityOnly()
        {
            // Arrange
            var cdnIntegrity = "sha384-kcAOn9fN4XSd+TGsNu2OQKSuV5ngOwt7tg73O4EpaD91QXvrfgvf0MR7/2dUjoI6";

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetCdnIntegrity(cdnIntegrity);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.NotNull(resource.CdnIntegrity);
            Assert.Null(resource.CdnDebugIntegrity);
        }

        [Fact]
        public void AbilityToSetCdnDebugIntegrityOnly()
        {
            // Arrange
            var cdnIntegrity = "sha384-xewr6kSkq3dBbEtB6Z/3oFZmknWn7nHqhLVLrYgzEFRbU/DHSxW7K3B44yWUN60D";

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetCdnIntegrity(cdnIntegrity, debug: true);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.Null(resource.CdnIntegrity);
            Assert.NotNull(resource.CdnDebugIntegrity);
        }

        [Fact]
        public void AbilityToSetBothCdnIntegrityAndCdnDebugIntegrity()
        {
            // Arrange
            var cdnIntegrity = "sha384-kcAOn9fN4XSd+TGsNu2OQKSuV5ngOwt7tg73O4EpaD91QXvrfgvf0MR7/2dUjoI6";
            var cdnDebugIntegrity = "sha384-xewr6kSkq3dBbEtB6Z/3oFZmknWn7nHqhLVLrYgzEFRbU/DHSxW7K3B44yWUN60D";

            // Act
            _resourceManifest
                .DefineScript("test")
                .SetCdnIntegrity(cdnIntegrity, cdnDebugIntegrity);

            // Assert
            var resource = _resourceManifest.GetResources("script").Values.ElementAt(0).ElementAt(0);

            Assert.NotNull(resource.CdnIntegrity);
            Assert.NotNull(resource.CdnDebugIntegrity);
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
