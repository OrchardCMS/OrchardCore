using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using OrchardCore.Modules;

namespace OrchardCore.Tests.Localization
{
    public class HtmlLocalizerExtensionsTests
    {
        [Theory]
        [InlineData(1, "1 minute ago", "{0} minutes ago", "1 minute ago")]
        [InlineData(20, "1 minute ago", "{0} minutes ago", "20 minutes ago")]
        public async void PluralFormShouldWorkWithNullStringLocalizer(int count, string singular, string plural, string expected)
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

                        Assert.Equal(expected, htmlLocalizer.Plural(count, singular, plural).Value);

                        return Task.FromResult(0);
                    });
                });

            using (var server = new TestServer(webHostBuilder))
            {
                var client = server.CreateClient();
                var response = await client.GetAsync("/");
            }
        }
    }
}