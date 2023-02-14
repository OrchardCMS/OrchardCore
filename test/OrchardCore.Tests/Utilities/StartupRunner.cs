using Microsoft.AspNetCore.Localization;

namespace OrchardCore.Tests
{
    internal static class StartupRunner
    {
        public static async Task Run(Type startupType, string culture, string expected)
        {
            var webHostBuilder = new WebHostBuilder().UseStartup(startupType);
            var testHost = new TestServer(webHostBuilder);

            var client = testHost.CreateClient();
            var request = new HttpRequestMessage();
            var cookieValue = $"c={culture}|uic={culture}";
            request.Headers.Add("Cookie", $"{CookieRequestCultureProvider.DefaultCookieName}={cookieValue}");

            var response = await client.SendAsync(request);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expected, await response.Content.ReadAsStringAsync());
        }
    }
}
