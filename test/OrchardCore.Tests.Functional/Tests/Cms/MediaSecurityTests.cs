using System.Text.Json;
using Microsoft.Playwright;
using OrchardCore.Tests.Functional.Helpers;
using Xunit;

namespace OrchardCore.Tests.Functional.Tests.Cms;

[Collection(CmsTestCollection.Name)]
public sealed class MediaSecurityTests : IAsyncLifetime
{
    private const int SmallFileSize = 100 * 1024;
    private const string Password = "Orchard1!";

    private readonly SaasFixture _fixture;
    private TenantInfo _tenant;

    public MediaSecurityTests(SaasFixture fixture)
    {
        _fixture = fixture;
    }

    public async ValueTask InitializeAsync()
    {
        _tenant = TestUtils.GenerateTenantInfo("Media");
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
    public async Task MediaApi_FolderPermissions_IsolateUsers()
    {
        const string alphaFolder = "Alpha";
        const string betaFolder = "Beta";
        const string alphaFile = "alpha.jpg";
        const string betaFile = "beta.jpg";
        var prefix = $"/{_tenant.Prefix}";

        var page = await _fixture.CreatePageAsync();
        await AuthHelper.LoginAsync(page, prefix);

        await CreateFolderWithFileAsync(page, prefix, alphaFolder, alphaFile);
        await CreateFolderWithFileAsync(page, prefix, betaFolder, betaFile);
        await FeatureHelper.EnableFeatureAsync(page, prefix, "OrchardCore.Media.Security");
        await SetRolePermissionAsync(page, prefix, "Anonymous", "ViewMediaContent", granted: false);

        await CreateRoleWithPermissionsAsync(page, prefix, "AlphaMedia",
            "ManageMediaContent", "ViewRootMediaContent", $"ViewMediaContent_{alphaFolder}");
        await CreateRoleWithPermissionsAsync(page, prefix, "BetaMedia",
            "ManageMediaContent", "ViewRootMediaContent", $"ViewMediaContent_{betaFolder}");

        await UserHelper.CreateUserAsync(page, prefix, "alpha-user", "alpha@test.com", Password, "AlphaMedia");
        await UserHelper.CreateUserAsync(page, prefix, "beta-user", "beta@test.com", Password, "BetaMedia");

        var scenarios = new[]
        {
            (UserName: "alpha-user", AllowedFolder: alphaFolder, AllowedFile: alphaFile, DeniedFolder: betaFolder, DeniedFile: betaFile),
            (UserName: "beta-user", AllowedFolder: betaFolder, AllowedFile: betaFile, DeniedFolder: alphaFolder, DeniedFile: alphaFile),
        };

        foreach (var scenario in scenarios)
        {
            await UserHelper.LoginAsAsync(page, prefix, scenario.UserName, Password);

            await AssertRootListingsAsync(
                page,
                scenario.AllowedFolder,
                scenario.DeniedFolder);

            await AssertDirectAccessAsync(
                page,
                scenario.AllowedFolder,
                scenario.AllowedFile,
                scenario.DeniedFolder,
                scenario.DeniedFile);
        }

        await page.CloseAsync();
    }

    private static async Task CreateFolderWithFileAsync(
        IPage page,
        string prefix,
        string folder,
        string fileName)
    {
        await MediaHelper.NavigateToMediaAsync(page, prefix);
        await MediaHelper.CreateFolderAsync(page, folder);
        await MediaHelper.NavigateToFolderAsync(page, folder);

        var filePath = MediaHelper.GenerateTestFile(fileName, SmallFileSize);
        await MediaHelper.UploadFileAsync(page, filePath);
        await MediaHelper.ExpectFileInLibraryAsync(page, fileName);
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
        string deniedFolder)
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

    private static string[] GetNames(JsonElement items)
        => items.EnumerateArray()
            .Select(item => item.GetProperty("name").GetString())
            .ToArray();
}
