using System.IO;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;
using Xunit;

namespace OrchardCore.Tests.Localization
{
    public class NullStringLocalizerTests
    {
        [Theory]
        [InlineData("Welcome to {0}!!", "Welcome to OrchardCore!!", "OrchardCore")]
        [InlineData("Welcome to {0} {1}!!", "Welcome to OrchardCore CMS!!", "OrchardCore", "CMS")]
        public async void LocalizerReturnsFormattedString(string name, string expected, params object[] arguments)
        {
            var webHostBuilder = new WebHostBuilder()
                .ConfigureServices(services =>
                {
                    services.AddOrchardCore();
                })
                .Configure(app =>
                {
                    app.UseOrchardCore();

                    app.Run(context =>
                    {
                        var htmlLocalizer = context.RequestServices.GetService<IHtmlLocalizer>();

                        Assert.Equal(expected, htmlLocalizer[name, arguments].Value);

                        return Task.FromResult(0);
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync("/");
            }
        }

        [Theory]
        [InlineData("there is one", 1, "there is one", "there are more", new object[0])]
        [InlineData("there are more", 2, "there is one", "there are more", new object[0])]
        [InlineData("there is one (1)", 1, "there is one ({0})", "there are more ({0})", new object[0])]
        [InlineData("there are more (2)", 2, "there is one ({0})", "there are more ({0})", new object[0])]
        [InlineData("there is one (1) thing", 1, "there is one ({0}) {1}", "there are more ({0}) {1}", new object[] { "thing" })]
        [InlineData("there are more (2) thing", 2, "there is one ({0}) {1}", "there are more ({0}) {1}", new object[] { "thing" })]
        [InlineData("there is one (1) &lt;br/&gt;", 1, "there is one ({0}) {1}", "there are more ({0}) {1}", new object[] { "<br/>" })]
        [InlineData("there are more (2) &lt;br/&gt;", 2, "there is one ({0}) {1}", "there are more ({0}) {1}", new object[] { "<br/>" })]
        public void HtmlNullLocalizerSupportsPlural(string expected, int count, string singular, string plural, object[] arguments)
        {
            IHtmlLocalizer localizer = NullLocalizerFactory.NullLocalizer.Instance;

            using (var writer = new StringWriter())
            {
                localizer.Plural(count, singular, plural, arguments).WriteTo(writer, HtmlEncoder.Default);
                Assert.Equal(expected, writer.ToString());
            }
        }
    }
}