using OrchardCore.Environment.Shell;
using OrchardCore.Media;
using OrchardCore.Tenants.Models;
using OrchardCore.Tests.Apis.Context;

namespace OrchardCore.Modules.OrchardCore.Tenants.Tests;

public class TenantApiPathContainmentTests
{
    [Theory]
    [InlineData("..")]
    [InlineData("../..")]
    [InlineData("../../bin")]
    [InlineData(@"..\..")]
    [InlineData("Invalid Tenant")]
    public async Task Create_TenantNameOutsideTenantsDirectory_ReturnsBadRequestWithoutCreatingTheTenant(string tenantName)
    {
        // Arrange
        await SiteContext.ShellHost.InitializeAsync();

        var model = new TenantApiModel
        {
            Name = tenantName,
            RequestUrlPrefix = Guid.NewGuid().ToString("n"),
            DatabaseProvider = "Sqlite",
            RecipeName = "Blog",
        };

        // Act
        var response = await SiteContext.DefaultTenantClient.PostAsJsonAsync("api/tenants/create", model);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(SiteContext.ShellHost.TryGetSettings(tenantName, out _));
    }

    [Fact]
    public async Task CreateAndSetup_ValidTenant_MediaRootIsInsideTheTenantDirectory()
    {
        // Arrange
        using var context = new SiteContext();

        // Act: creates and sets up a tenant through 'api/tenants/create' and 'api/tenants/setup'.
        await context.InitializeAsync();

        // Assert
        await context.UsingTenantScopeAsync(scope =>
        {
            var shellOptions = scope.ServiceProvider.GetRequiredService<IOptions<ShellOptions>>().Value;
            var tenantPath = Path.GetFullPath(Path.Combine(
                shellOptions.ShellsApplicationDataPath,
                shellOptions.ShellsContainerName,
                context.TenantName)) + Path.DirectorySeparatorChar;

            var mediaFileProvider = Assert.IsAssignableFrom<PhysicalFileProvider>(
                scope.ServiceProvider.GetRequiredService<IMediaFileProvider>());

            Assert.StartsWith(tenantPath, Path.GetFullPath(mediaFileProvider.Root), StringComparison.OrdinalIgnoreCase);

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CreateAndSetup_ValidTenant_MediaUploadIsStoredInsideTheTenantDirectory()
    {
        // Arrange
        using var context = new SiteContext();
        await context.InitializeAsync();

        var fileName = Guid.NewGuid().ToString("n") + ".txt";

        // Act
        await context.UsingTenantScopeAsync(async scope =>
        {
            var mediaFileStore = scope.ServiceProvider.GetRequiredService<IMediaFileStore>();

            using var stream = new MemoryStream("containment"u8.ToArray());
            await mediaFileStore.CreateFileFromStreamAsync(fileName, stream);
        });

        // Assert
        await context.UsingTenantScopeAsync(scope =>
        {
            var shellOptions = scope.ServiceProvider.GetRequiredService<IOptions<ShellOptions>>().Value;
            var tenantPath = Path.GetFullPath(Path.Combine(
                shellOptions.ShellsApplicationDataPath,
                shellOptions.ShellsContainerName,
                context.TenantName));

            var matches = Directory.GetFiles(tenantPath, fileName, SearchOption.AllDirectories);

            Assert.Single(matches);

            return Task.CompletedTask;
        });
    }
}
