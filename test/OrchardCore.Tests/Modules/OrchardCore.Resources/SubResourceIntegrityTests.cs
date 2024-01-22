using System.Security.Cryptography;
using OrchardCore.ResourceManagement;
using OrchardCore.Resources;

namespace OrchardCore.Tests.Modules.OrchardCore.Resources
{
    public class SubResourceIntegrityTests
    {
        [Fact]
        public void SavedSubResourceIntegritiesShouldMatchCurrentResources()
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

            ValidateSubResourceIntegrity("script");
            ValidateSubResourceIntegrity("style");

            void ValidateSubResourceIntegrity(string resourceType)
            {
                foreach (var resource in resourceManifest.GetResources(resourceType))
                {
                    foreach (var resourceDefinition in resource.Value)
                    {
                        if (!string.IsNullOrEmpty(resourceDefinition.CdnIntegrity) && !string.IsNullOrEmpty(resourceDefinition.UrlCdnDebug))
                        {
                            var resourceIntegrity = GetSubResourceIntegrityAsync(resourceDefinition.UrlCdnDebug).Result;

                            Assert.True(resourceIntegrity.Equals(resourceDefinition.CdnDebugIntegrity), $"The {resourceType} {resourceDefinition.UrlCdnDebug} has invalid SRI hash, please use '{resourceIntegrity}' instead.");
                        }

                        if (!string.IsNullOrEmpty(resourceDefinition.CdnIntegrity) && !string.IsNullOrEmpty(resourceDefinition.UrlCdn))
                        {
                            var resourceIntegrity = GetSubResourceIntegrityAsync(resourceDefinition.UrlCdn).Result;

                            Assert.True(resourceIntegrity.Equals(resourceDefinition.CdnIntegrity), $"The {resourceType} {resourceDefinition.UrlCdn} has invalid SRI hash, please use '{resourceIntegrity}' instead.");
                        }
                    }
                }
            }
        }

        private static async Task<string> GetSubResourceIntegrityAsync(string url)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(url)
            };

            var data = await client.GetByteArrayAsync(url);

            using var memoryStream = new MemoryStream(data);
            using var sha384Hash = SHA384.Create();
            var hash = sha384Hash.ComputeHash(memoryStream);

            return "sha384-" + Convert.ToBase64String(hash);
        }
    }
}
