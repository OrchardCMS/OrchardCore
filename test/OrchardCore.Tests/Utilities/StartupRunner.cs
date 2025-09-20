using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using Xunit;

namespace OrchardCore.Tests;

internal static class StartupRunner
{
    public static async Task Run(Type startupType, string culture, string expected)
    {
        var builder = Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseTestServer()
                          .UseStartup(startupType);
            });

        using var host = await builder.StartAsync();
        var client = host.GetTestClient();
        
        var request = new HttpRequestMessage();
        var cookieValue = $"c={culture}|uic={culture}";
        request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cookieValue}");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, await response.Content.ReadAsStringAsync());
    }
}
