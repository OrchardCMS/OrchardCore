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
        var builder = WebApplication.CreateBuilder();
        
        // Configure the startup type (assuming it implements IStartup or has Configure/ConfigureServices methods)
        var startupInstance = Activator.CreateInstance(startupType);
        
        // Call ConfigureServices if it exists
        var configureServicesMethod = startupType.GetMethod("ConfigureServices");
        configureServicesMethod?.Invoke(startupInstance, [builder.Services]);
        
        var app = builder.Build();
        
        // Call Configure if it exists
        var configureMethod = startupType.GetMethod("Configure");
        configureMethod?.Invoke(startupInstance, [app]);
        
        // Create TestServer with the app's ServiceProvider (non-obsolete constructor)
        using var testHost = new TestServer(app.Services);

        var client = testHost.CreateClient();
        var request = new HttpRequestMessage();
        var cookieValue = $"c={culture}|uic={culture}";
        request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cookieValue}");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expected, await response.Content.ReadAsStringAsync());
    }
}
