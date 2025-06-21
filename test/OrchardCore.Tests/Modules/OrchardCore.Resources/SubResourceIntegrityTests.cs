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
        await ValidateSubResourceIntegrityAsync("script").ConfigureAwait(false);
        await ValidateSubResourceIntegrityAsync("style").ConfigureAwait(false);

        async Task ValidateSubResourceIntegrityAsync(string resourceType)
        {
            foreach (var resource in resourceManifest.GetResources(resourceType))
            {
                foreach (var resourceDefinition in resource.Value)
                {
                    if (!string.IsNullOrEmpty(resourceDefinition.CdnIntegrity) && !string.IsNullOrEmpty(resourceDefinition.UrlCdnDebug))
                    {
                        var resourceIntegrity = await GetSubResourceIntegrityAsync(httpClient, resourceDefinition.UrlCdnDebug).ConfigureAwait(false);

                        Assert.True(resourceIntegrity.Equals(resourceDefinition.CdnDebugIntegrity, StringComparison.Ordinal),
                            $"The {resourceType} {resourceDefinition.UrlCdnDebug} has invalid SRI hash, please use '{resourceIntegrity}' instead.");
                    }

                    if (!string.IsNullOrEmpty(resourceDefinition.CdnIntegrity) && !string.IsNullOrEmpty(resourceDefinition.UrlCdn))
                    {
                        var resourceIntegrity = await GetSubResourceIntegrityAsync(httpClient, resourceDefinition.UrlCdn).ConfigureAwait(false);

                        Assert.True(resourceIntegrity.Equals(resourceDefinition.CdnIntegrity, StringComparison.Ordinal),
                            $"The {resourceType} {resourceDefinition.UrlCdn} has invalid SRI hash, please use '{resourceIntegrity}' instead.");
                    }
                }
            }
        }
    }

    private static async Task<string> GetSubResourceIntegrityAsync(HttpClient httpClient, string url)
    {
        var data = await httpClient.GetByteArrayAsync(url).ConfigureAwait(false);

        using var memoryStream = new MemoryStream(data);
        var hash = await SHA384.HashDataAsync(memoryStream).ConfigureAwait(false);

        return "sha384-" + Convert.ToBase64String(hash);
    }
}
