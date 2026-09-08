using System.Text.Json;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class MediaSecurityTests : IAsyncLifetime
{
    private const string Password = "Orchard1!";

    private readonly SaasFixture _fixture;
    private TenantInfo _tenant;

    public MediaSecurityTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("MediaSecurity");
        var page = await _fixture.CreatePageAsync();
        await TenantHelper.NewTenantAsync(page, _tenant);
        await page.CloseAsync();
    }

    public ValueTask DisposeAsync()
    {
        MediaHelper.CleanupTestFiles();
        return ValueTask.CompletedTask;
    }

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
    public async Task MediaApi_FolderPermissions_IsolateUsers()
    {
        const string alphaFolder = "alpha";
        const string betaFolder = "beta";
        const string alphaFile = "alpha.png";
        const string betaFile = "beta.png";
        var prefix = $"/{_tenant.Prefix}";

        var adminPage = await _fixture.CreatePageAsync();
        await adminPage.LoginAsync(prefix);
        await CreateRoleWithPermissionsAsync(adminPage, prefix, "AlphaMediaApi",
            "ManageMediaContent", "ViewRootMediaContent", $"ViewMediaContent_{alphaFolder}");
        await CreateRoleWithPermissionsAsync(adminPage, prefix, "BetaMediaApi",
            "ManageMediaContent", "ViewRootMediaContent", $"ViewMediaContent_{betaFolder}");
        await UserHelper.CreateUserAsync(adminPage, prefix, "api-alpha-user", "api-alpha@test.com", Password, "AlphaMediaApi");
        await UserHelper.CreateUserAsync(adminPage, prefix, "api-beta-user", "api-beta@test.com", Password, "BetaMediaApi");
        await adminPage.CloseAsync();

        var page = await _fixture.CreatePageAsync();

        var scenarios = new[]
        {
            (UserName: "api-alpha-user", AllowedFolder: alphaFolder, AllowedFile: alphaFile, DeniedFolder: betaFolder, DeniedFile: betaFile),
            (UserName: "api-beta-user", AllowedFolder: betaFolder, AllowedFile: betaFile, DeniedFolder: alphaFolder, DeniedFile: alphaFile),
        };

        foreach (var scenario in scenarios)
        {
            await UserHelper.LoginAsAsync(page, prefix, scenario.UserName, Password);

            await AssertRootListingsAsync(
                page,
                scenario.AllowedFolder,
                scenario.AllowedFile,
                scenario.DeniedFolder,
                scenario.DeniedFile);

            await AssertDirectAccessAsync(
                page,
                scenario.AllowedFolder,
                scenario.AllowedFile,
                scenario.DeniedFolder,
                scenario.DeniedFile);
        }

        await page.CloseAsync();
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

    private static async Task CreateRoleWithPermissionsAsync(
        IPage page,
        string prefix,
        string roleName,
        params string[] permissionNames)
    {
        await page.GotoAsync($"{prefix}/Admin/Roles/Create");
        await page.Locator("input[name='RoleName']").FillAsync(roleName);
        await page.ClickCreateAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        foreach (var permissionName in permissionNames)
        {
            await SetRolePermissionAsync(page, prefix, roleName, permissionName, granted: true);
        }
    }

    private static async Task SetRolePermissionAsync(
        IPage page,
        string prefix,
        string roleName,
        string permissionName,
        bool granted)
    {
        await page.GotoAsync($"{prefix}/Admin/Roles/Edit/{Uri.EscapeDataString(roleName)}");
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        var checkbox = page.Locator($"input[id='Checkbox.{permissionName}']");
        await checkbox.WaitForAsync(new() { State = WaitForSelectorState.Attached, Timeout = 10_000 });

        if (granted)
        {
            await checkbox.CheckAsync();
        }
        else
        {
            await checkbox.UncheckAsync();
        }

        await page.ClickSaveAsync();
        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task AssertRootListingsAsync(
        IPage page,
        string allowedFolder,
        string allowedFile,
        string deniedFolder,
        string deniedFile)
    {
        var treeResponse = await GetAsync(page, "api/media/GetDirectoryTree");
        Assert.Equal(200, treeResponse.Status);
        using (var tree = JsonDocument.Parse(await treeResponse.TextAsync()))
        {
            var folders = GetNames(tree.RootElement.GetProperty("children"));
            Assert.Contains(allowedFolder, folders);
            Assert.DoesNotContain(deniedFolder, folders);
        }

        var contentResponse = await GetAsync(page, "api/media/GetDirectoryContent?path=&extensions=");
        Assert.Equal(200, contentResponse.Status);
        using (var content = JsonDocument.Parse(await contentResponse.TextAsync()))
        {
            var folders = GetNames(content.RootElement.GetProperty("folders"));
            Assert.Contains(allowedFolder, folders);
            Assert.DoesNotContain(deniedFolder, folders);
        }

        var foldersResponse = await GetAsync(page, "api/media/GetFolders?path=");
        Assert.Equal(200, foldersResponse.Status);
        using (var folders = JsonDocument.Parse(await foldersResponse.TextAsync()))
        {
            var names = GetNames(folders.RootElement.GetProperty("items"));
            Assert.Contains(allowedFolder, names);
            Assert.DoesNotContain(deniedFolder, names);
        }

        var allItemsResponse = await GetAsync(page, "api/media/GetAllMediaItems?extensions=");
        Assert.Equal(200, allItemsResponse.Status);
        using (var allItems = JsonDocument.Parse(await allItemsResponse.TextAsync()))
        {
            var names = GetNames(allItems.RootElement);
            Assert.Contains(allowedFolder, names);
            Assert.Contains(allowedFile, names);
            Assert.DoesNotContain(deniedFolder, names);
            Assert.DoesNotContain(deniedFile, names);
        }
    }

    private async Task AssertDirectAccessAsync(
        IPage page,
        string allowedFolder,
        string allowedFile,
        string deniedFolder,
        string deniedFile)
    {
        var allowedFolderResponse = await GetAsync(
            page,
            $"api/media/GetMediaItems?path={Uri.EscapeDataString(allowedFolder)}&extensions=");
        Assert.Equal(200, allowedFolderResponse.Status);
        using (var items = JsonDocument.Parse(await allowedFolderResponse.TextAsync()))
        {
            Assert.Contains(allowedFile, GetNames(items.RootElement));
        }

        var deniedFolderResponse = await GetAsync(
            page,
            $"api/media/GetMediaItems?path={Uri.EscapeDataString(deniedFolder)}&extensions=");
        Assert.Equal(403, deniedFolderResponse.Status);

        var deniedDirectoryContentResponse = await GetAsync(
            page,
            $"api/media/GetDirectoryContent?path={Uri.EscapeDataString(deniedFolder)}&extensions=");
        Assert.Equal(403, deniedDirectoryContentResponse.Status);

        var deniedFoldersResponse = await GetAsync(
            page,
            $"api/media/GetFolders?path={Uri.EscapeDataString(deniedFolder)}");
        Assert.Equal(403, deniedFoldersResponse.Status);

        var allowedPath = Uri.EscapeDataString($"{allowedFolder}/{allowedFile}");
        var deniedPath = Uri.EscapeDataString($"{deniedFolder}/{deniedFile}");

        var allowedItemResponse = await GetAsync(page, $"api/media/GetMediaItem?path={allowedPath}");
        Assert.Equal(200, allowedItemResponse.Status);

        var deniedItemResponse = await GetAsync(page, $"api/media/GetMediaItem?path={deniedPath}");
        Assert.Equal(403, deniedItemResponse.Status);

        var mixedFieldItemsResponse = await GetAsync(
            page,
            $"api/media/GetMediaFieldItems?paths={allowedPath}&paths={deniedPath}");
        Assert.Equal(403, mixedFieldItemsResponse.Status);
    }

    private Task<IAPIResponse> GetAsync(IPage page, string relativeUrl)
        => page.APIRequest.GetAsync(
            $"{_fixture.BaseUrl}/{_tenant.Prefix}/{relativeUrl}",
            new APIRequestContextOptions { MaxRedirects = 0 });

    private static async Task AssertMediaStatusAsync(IPage page, string prefix, string folder, int expectedStatus)
    {
        var response = await page.GotoAsync($"{prefix}/media/{folder}/{folder}.png");

        Assert.Equal(expectedStatus, response.Status);
    }

    private static string[] GetNames(JsonElement items)
        => items.EnumerateArray()
            .Select(item => item.GetProperty("name").GetString())
            .ToArray();
}
