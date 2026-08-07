using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class MediaFolderPermissionTests : IAsyncLifetime
{
    private const string Password = "Orchard1!";

    private readonly SaasFixture _fixture;
    private TenantInfo _tenant;

    public MediaFolderPermissionTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("MediaSecurity");
        var page = await _fixture.CreatePageAsync();
        await page.NewTenantAsync(_tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task MediaFolders_AreAccessibleOnlyToUsersWithMatchingPermissions()
    {
        var prefix = $"/{_tenant.Prefix}";
        var adminPage = await _fixture.CreatePageAsync();

        await adminPage.LoginAsync(prefix);
        await UserHelper.CreateUserAsync(adminPage, prefix, "alpha-user", "alpha@orchard.test", Password, "AlphaMediaViewer");
        await UserHelper.CreateUserAsync(adminPage, prefix, "beta-user", "beta@orchard.test", Password, "BetaMediaViewer");
        await UserHelper.CreateUserAsync(adminPage, prefix, "all-media-user", "all-media@orchard.test", Password, "AllMediaViewer");

        await adminPage.CloseAsync();

        await AssertFolderAccessAsync("alpha-user", allowedFolder: "alpha", deniedFolder: "beta");
        await AssertFolderAccessAsync("beta-user", allowedFolder: "beta", deniedFolder: "alpha");
        await AssertAllFoldersAccessibleAsync("all-media-user");
    }

    [Fact]
    public async Task FolderScopedRole_CanReachRootAuthorizedEndpoints()
    {
        // https://github.com/OrchardCMS/OrchardCore/issues/19675
        //
        // Some media endpoints authorize ManageMediaFolder against the root path — GetPermittedStorage
        // and the media admin page among them. SecureMediaPermissions publishes a ViewRootMediaContent
        // permission implied by every first-level folder permission precisely so that a folder-scoped
        // role still passes those checks. When the handler authorizes against the static permission
        // instance instead, it does not, and a user who can manage 'alpha' cannot open the Media library
        // at all — while the role editor shows the access as granted.
        var prefix = $"/{_tenant.Prefix}";
        var adminPage = await _fixture.CreatePageAsync();

        await adminPage.LoginAsync(prefix);
        await UserHelper.CreateUserAsync(adminPage, prefix, "root-scoped-user", "root-scoped@orchard.test", Password, "AlphaMediaViewer");
        await adminPage.CloseAsync();

        var page = await _fixture.CreatePageAsync();

        await UserHelper.LoginAsAsync(page, prefix, "root-scoped-user", Password);

        var status = await GetStatusAsync(page, $"{prefix}/api/media/GetPermittedStorage");

        Assert.Equal(200, status);

        await page.CloseAsync();
    }

    private static Task<int> GetStatusAsync(IPage page, string url)
    {
        const string getStatusScript =
            """
            async url => {
                const response = await fetch(url);

                return response.status;
            }
            """;

        return page.EvaluateAsync<int>(getStatusScript, url);
    }

    private async Task AssertFolderAccessAsync(string userName, string allowedFolder, string deniedFolder)
    {
        var page = await _fixture.CreatePageAsync();
        var prefix = $"/{_tenant.Prefix}";

        await UserHelper.LoginAsAsync(page, prefix, userName, Password);

        await MediaHelper.NavigateToMediaAsync(page, prefix);
        await AssertFolderManagementAsync(page, prefix, allowedFolder, deniedFolder);
        await AssertMediaStatusAsync(page, prefix, allowedFolder, expectedStatus: 200);
        await AssertMediaStatusAsync(page, prefix, deniedFolder, expectedStatus: 404);

        await page.CloseAsync();
    }

    private async Task AssertAllFoldersAccessibleAsync(string userName)
    {
        var page = await _fixture.CreatePageAsync();
        var prefix = $"/{_tenant.Prefix}";

        await UserHelper.LoginAsAsync(page, prefix, userName, Password);

        await AssertMediaStatusAsync(page, prefix, "alpha", expectedStatus: 200);
        await AssertMediaStatusAsync(page, prefix, "beta", expectedStatus: 200);

        await page.CloseAsync();
    }

    private static async Task AssertFolderManagementAsync(IPage page, string prefix, string allowedFolder, string deniedFolder)
    {
        var allowedStatus = await CreateFolderAsync(page, prefix, allowedFolder, "allowed-child");
        var deniedStatus = await CreateFolderAsync(page, prefix, deniedFolder, "denied-child");

        Assert.Equal(200, allowedStatus);
        Assert.Equal(403, deniedStatus);
    }

    private static Task<int> CreateFolderAsync(IPage page, string prefix, string parentFolder, string folderName)
    {
        const string createFolderScript =
            """
            async url => {
                const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
                const response = await fetch(url, {
                    method: "POST",
                    headers: { RequestVerificationToken: token }
                });

                return response.status;
            }
            """;

        var url = $"{prefix}/api/media/CreateFolder?path={parentFolder}&name={folderName}";

        return page.EvaluateAsync<int>(createFolderScript, url);
    }

    private static async Task AssertMediaStatusAsync(IPage page, string prefix, string folder, int expectedStatus)
    {
        var response = await page.GotoAsync($"{prefix}/media/{folder}/{folder}.png");

        Assert.Equal(expectedStatus, response.Status);
    }
}
