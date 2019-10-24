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
        [InlineData("there is one", 1, "there is one", "there are more")]
        [InlineData("there are more", 2, "there is one", "there are more")]
        [InlineData("there is one (1)", 1, "there is one ({0})", "there are more ({0})")]
        [InlineData("there are more (2)", 2, "there is one ({0})", "there are more ({0})")]
        [InlineData("there is one (1) thing", 1, "there is one ({0}) {1}", "there are more ({0}) {1}", "thing")]
        [InlineData("there are more (2) thing", 2, "there is one ({0}) {1}", "there are more ({0}) {1}", "thing")]
        [InlineData("there is one (1) &lt;br/&gt;", 1, "there is one ({0}) {1}", "there are more ({0}) {1}", "<br/>")]
        [InlineData("there are more (2) &lt;br/&gt;", 2, "there is one ({0}) {1}", "there are more ({0}) {1}", "<br/>")]
        public void HtmlNullLocalizerSupportsPlural(string expected, int count, string singular, string plural, params object[] arguments)
        {
            var localizer = ((IHtmlLocalizerFactory)new NullLocalizerFactory()).Create(typeof(object));

            using (var writer = new StringWriter())
            {
                localizer.Plural(count, singular, plural, arguments).WriteTo(writer, HtmlEncoder.Default);
                Assert.Equal(expected, writer.ToString());
            }
        }

        [Theory]
        [InlineData("there is one", 1, "there is one", "there are more")]
        [InlineData("there are more", 2, "there is one", "there are more")]
        [InlineData("there is one (1)", 1, "there is one ({0})", "there are more ({0})")]
        [InlineData("there are more (2)", 2, "there is one ({0})", "there are more ({0})")]
        [InlineData("there is one (1) thing", 1, "there is one ({0}) {1}", "there are more ({0}) {1}", "thing")]
        [InlineData("there are more (2) thing", 2, "there is one ({0}) {1}", "there are more ({0}) {1}", "thing")]
        [InlineData("there is one (1) <br/>", 1, "there is one ({0}) {1}", "there are more ({0}) {1}", "<br/>")]
        [InlineData("there are more (2) <br/>", 2, "there is one ({0}) {1}", "there are more ({0}) {1}", "<br/>")]
        public void StringNullLocalizerSupportsPlural(string expected, int count, string singular, string plural, params object[] arguments)
        {
            var localizer = new NullLocalizerFactory().Create(typeof(object));

            var value = localizer.Plural(count, singular, plural, arguments).Value;
            Assert.Equal(expected, value);
        }
    }
}
