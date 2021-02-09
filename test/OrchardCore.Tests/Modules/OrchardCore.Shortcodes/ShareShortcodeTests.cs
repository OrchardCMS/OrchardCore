using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.FileStorage;
using OrchardCore.Infrastructure.Html;
using OrchardCore.Media.Core;
using OrchardCore.Media.Events;
using OrchardCore.Media.Shortcodes;
using OrchardCore.ResourceManagement;
using OrchardCore.Shortcodes.Services;
using Shortcodes;
using Xunit;

namespace OrchardCore.Tests.Modules.OrchardCore.Shortcodes
{
    public class ShareShortcodeTests
    {
        [Theory]
        [InlineData("", "foo bar baz", "foo bar baz")]
        [InlineData("", "[share]", "[share]")]
        [InlineData("", "[share][/share]", "[share]")]
        [InlineData("", "[download url=design.doc]Design[/download]", @"<a href=""/media/design.doc"" download=""design.doc"">Design</a>")]
        [InlineData("", "[doc url=design.doc]Design[/doc]", @"<a href=""/media/design.doc"" download=""design.doc"">Design</a>")]
        [InlineData("", "[link url=design.doc]Design[/link]", @"[link url=design.doc]Design[/link]")]
        [InlineData("", "[share url=design.doc]Design[/share]", @"<a href=""/media/design.doc"" download=""design.doc"">Design</a>")]
        [InlineData("", "[share]/docs/testing.xml[/share]", @"<a href=""/media/docs/testing.xml"" download=""testing.xml"">testing</a>")]
        [InlineData("", "[share url=/customer/support.odt]", @"<a href=""/media/customer/support.odt"" download=""support.odt"">support</a>")]
        [InlineData("", "[share url=/customer/support.odt tooltip=\"Customer Support\" class=\"big bold\"]", @"<a title=""Customer Support"" class=""big bold"" href=""/media/customer/support.odt"" download=""support.odt"">support</a>")]
        [InlineData("", "[share url=/customer/support.odt save=\"cs.odt\"]", @"<a href=""/media/customer/support.odt"" download=""cs.odt"">support</a>")]
        [InlineData("", "[share url=http://example.com/customer/support.odt save=\"cs.odt\"]", @"<a href=""http://example.com/customer/support.odt"" download=""cs.odt"">support</a>")]
        [InlineData("", "[share url=//example_domain_server/customer/support.odt save=\"cs.odt\"]", @"<a href=""//example_domain_server/customer/support.odt"" download=""cs.odt"">support</a>")]
        [InlineData("", "[share url=~/customer/support.odt save=\"cs.odt\"]", @"<a href=""/tenant/customer/support.odt"" download=""cs.odt"">support</a>")]
        public async Task ShouldProcess(string cdnBaseUrl, string content, string expected)
        {
            var sanitizerOptions = new HtmlSanitizerOptions();
            sanitizerOptions.Configure.Add(opt =>
            {
                _ = opt.AllowedAttributes.Add("class");
                _ = opt.AllowedAttributes.Add("download");
            });

            var fileStore = new DefaultMediaFileStore(
                Mock.Of<IFileStore>(),
                "/media",
                cdnBaseUrl,
                Enumerable.Empty<IMediaEventHandler>(),
                Enumerable.Empty<IMediaCreatingEventHandler>(),
                Mock.Of<ILogger<DefaultMediaFileStore>>());

            var sanitizer = new HtmlSanitizerService(Options.Create(sanitizerOptions));
            
            var defaultHttpContext = new DefaultHttpContext();
            defaultHttpContext.Request.PathBase = new PathString("/tenant");
            var httpContextAccessor = Mock.Of<IHttpContextAccessor>(hca => hca.HttpContext == defaultHttpContext);

            var options = Options.Create(new ResourceManagementOptions { CdnBaseUrl = cdnBaseUrl });


            var shareProvider = new ShareShortCodeProvider(fileStore, sanitizer, httpContextAccessor, options);
            var processor = new ShortcodeService(new IShortcodeProvider[] { shareProvider }, Enumerable.Empty<IShortcodeContextProvider>());
            var processed = await processor.ProcessAsync(content);

            Assert.Equal(expected, processed);
        }
    }
}
