using System.Security.Cryptography;
using OrchardCore.ResourceManagement;
using OrchardCore.Resources;

namespace OrchardCore.Tests.Modules.OrchardCore.Resources;

public class SubResourceIntegrityTests
{
    [CIFact]
    public static async Task SavedSubResourceIntegritiesShouldMatchCurrentResources()
    {
        // Arrange
        var resourceOptions = Options.Create(new ResourceOptions());
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock
            .Setup(a => a.HttpContext)
            .Returns(new DefaultHttpContext());
        var configurationOptions = new ResourceManagementOptionsConfiguration(
            resourceOptions,
            Mock.Of<IHostEnvironment>(),
            httpContextAccessorMock.Object);
        var resourceManagementOptions = new ResourceManagementOptions();

        // Act
        configurationOptions.Configure(resourceManagementOptions);

        // Assert
        var resourceManifest = resourceManagementOptions.ResourceManifests.First();

        using var httpClient = new HttpClient();
        await ValidateSubResourceIntegrityAsync("script");
        await ValidateSubResourceIntegrityAsync("style");

        async Task ValidateSubResourceIntegrityAsync(string resourceType)
        {
            var expectations = new List<Tuple<string, string, string>>();

            foreach (var resource in resourceManifest.GetResources(resourceType))
            {
                foreach (var resourceDefinition in resource.Value)
                {
                    if (!string.IsNullOrEmpty(resourceDefinition.CdnDebugIntegrity) && !string.IsNullOrEmpty(resourceDefinition.UrlCdnDebug))
                    {
                        var resourceIntegrity = await GetSubResourceIntegrityAsync(httpClient, resourceDefinition.UrlCdnDebug);
                        expectations.Add(new Tuple<string, string, string>(resourceDefinition.UrlCdnDebug, resourceDefinition.CdnDebugIntegrity, resourceIntegrity));
                    }

                    if (!string.IsNullOrEmpty(resourceDefinition.CdnIntegrity) && !string.IsNullOrEmpty(resourceDefinition.UrlCdn))
                    {
                        var resourceIntegrity = await GetSubResourceIntegrityAsync(httpClient, resourceDefinition.UrlCdn);
                        expectations.Add(new Tuple<string, string, string>(resourceDefinition.UrlCdn, resourceDefinition.CdnIntegrity, resourceIntegrity));
                    }
                }
            }

            Assert.All(expectations, expectation => Assert.True(!string.IsNullOrEmpty(expectation.Item3), $"The {resourceType} {expectation.Item1} was not found (404 error). It is a non-valid url."));
            Assert.All(expectations, expectation => Assert.True(expectation.Item3.Equals(expectation.Item2, StringComparison.OrdinalIgnoreCase), $"The {resourceType} {expectation.Item1} has invalid SRI hash, please use '{expectation.Item3}' instead."));
        }
    }

    private static async Task<string> GetSubResourceIntegrityAsync(HttpClient httpClient, string url)
    {
        byte[] data;

        try
        {
            data = await httpClient.GetByteArrayAsync(url);
        }
        catch
        {
            return null;
        }

        using var memoryStream = new MemoryStream(data);
        var hash = await SHA384.HashDataAsync(memoryStream);
        return "sha384-" + Convert.ToBase64String(hash);
    }
}
