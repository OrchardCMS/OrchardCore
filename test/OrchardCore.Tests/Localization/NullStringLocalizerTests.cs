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
    }
}