using System.Security.Cryptography;
using System.IO;
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
            foreach (var resource in resourceManifest.GetResources(resourceType))
            {
                foreach (var resourceDefinition in resource.Value)
                {
                    if (!string.IsNullOrEmpty(resourceDefinition.CdnIntegrity) && !string.IsNullOrEmpty(resourceDefinition.UrlCdnDebug))
                    {
                        var resourceIntegrity = await GetSubResourceIntegrityAsync(
                            httpClient,
                            resourceDefinition.UrlCdnDebug,
                            resourceDefinition.UrlDebug ?? resourceDefinition.Url);

                        Assert.True(resourceIntegrity.Equals(resourceDefinition.CdnDebugIntegrity, StringComparison.Ordinal),
                            $"The debug {resourceType} {resourceDefinition.UrlCdnDebug} has invalid SRI hash, please use '{resourceIntegrity}' instead.");
                    }

                    if (!string.IsNullOrEmpty(resourceDefinition.CdnIntegrity) && !string.IsNullOrEmpty(resourceDefinition.UrlCdn))
                    {
                        var resourceIntegrity = await GetSubResourceIntegrityAsync(
                            httpClient,
                            resourceDefinition.UrlCdn,
                            resourceDefinition.Url ?? resourceDefinition.UrlDebug);

                        Assert.True(resourceIntegrity.Equals(resourceDefinition.CdnIntegrity, StringComparison.Ordinal),
                            $"The production {resourceType} {resourceDefinition.UrlCdn} has invalid SRI hash, please use '{resourceIntegrity}' instead.");
                    }
                }
            }
        }
    }

    private static async Task<string> GetSubResourceIntegrityAsync(HttpClient httpClient, string url, string fallbackUrl)
    {
        try
        {
            return await GetSubResourceIntegrityAsync(httpClient, url);
        }
        catch (Exception exception) when (exception is HttpRequestException or OperationCanceledException)
        {
            var localPath = GetLocalResourcePath(fallbackUrl);
            if (string.IsNullOrEmpty(localPath) || !File.Exists(localPath))
            {
                throw new InvalidOperationException(
                    $"Unable to retrieve '{url}' and no local fallback was found.",
                    exception);
            }

            return await GetSubResourceIntegrityFromFileAsync(localPath);
        }
    }

    private static async Task<string> GetSubResourceIntegrityAsync(HttpClient httpClient, string url)
    {
        var data = await httpClient.GetByteArrayAsync(url);

        using var memoryStream = new MemoryStream(data);
        var hash = await SHA384.HashDataAsync(memoryStream);

        return "sha384-" + Convert.ToBase64String(hash);
    }

    private static async Task<string> GetSubResourceIntegrityFromFileAsync(string filePath)
    {
        await using var fileStream = File.OpenRead(filePath);
        var hash = await SHA384.HashDataAsync(fileStream);

        return "sha384-" + Convert.ToBase64String(hash);
    }

    private static string GetLocalResourcePath(string url)
    {
        const string resourcePrefix = "~/OrchardCore.Resources/";
        if (string.IsNullOrEmpty(url) || !url.StartsWith(resourcePrefix, StringComparison.OrdinalIgnoreCase))
        {
            return string.Empty;
        }

        var repositoryRoot = GetRepositoryRoot();
        if (string.IsNullOrEmpty(repositoryRoot))
        {
            return string.Empty;
        }
        var relativePath = url[resourcePrefix.Length..].Replace('/', Path.DirectorySeparatorChar);

        return Path.Combine(repositoryRoot, "src", "OrchardCore.Modules", "OrchardCore.Resources", "wwwroot", relativePath);
    }

    private static string GetRepositoryRoot()
    {
        var environmentRoot = System.Environment.GetEnvironmentVariable("ORCHARDCORE_REPO_ROOT");
        if (!string.IsNullOrWhiteSpace(environmentRoot) && Directory.Exists(environmentRoot))
        {
            return environmentRoot;
        }

        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")) ||
                File.Exists(Path.Combine(directory.FullName, "OrchardCore.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return string.Empty;
    }
}
