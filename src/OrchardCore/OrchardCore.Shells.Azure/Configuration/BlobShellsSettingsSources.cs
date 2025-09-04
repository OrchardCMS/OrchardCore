using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Shells.Azure.Services;

namespace OrchardCore.Shells.Azure.Configuration;

public class BlobShellsSettingsSources : IShellsSettingsSources
{
    private readonly IShellsFileStore _shellsFileStore;
    private readonly BlobShellStorageOptions _blobOptions;

    private readonly string _tenantsFileSystemName;

    public BlobShellsSettingsSources(IShellsFileStore shellsFileStore,
        BlobShellStorageOptions blobOptions,
        IOptions<ShellOptions> shellOptions)
    {
        _shellsFileStore = shellsFileStore;
        _blobOptions = blobOptions;
        _tenantsFileSystemName = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, OrchardCoreConstants.Shell.TenantsFileName);
    }

    public async Task AddSourcesAsync(IConfigurationBuilder builder)
    {
        var fileInfo = await _shellsFileStore.GetFileInfoAsync(OrchardCoreConstants.Shell.TenantsFileName);

        if (fileInfo == null && _blobOptions.MigrateFromFiles)
        {
            if (await TryMigrateFromFileAsync())
            {
                fileInfo = await _shellsFileStore.GetFileInfoAsync(OrchardCoreConstants.Shell.TenantsFileName);
            }
            else
            {
                return;
            }
        }

        if (fileInfo != null)
        {
            var stream = await _shellsFileStore.GetFileStreamAsync(OrchardCoreConstants.Shell.TenantsFileName);
            builder.AddTenantJsonStream(stream);
        }
    }

    public Task AddSourcesAsync(string tenant, IConfigurationBuilder builder) => AddSourcesAsync(builder);

    public async Task SaveAsync(string tenant, IDictionary<string, string> data)
    {
        JsonObject tenantsSettings;

        var fileInfo = await _shellsFileStore.GetFileInfoAsync(OrchardCoreConstants.Shell.TenantsFileName);

        if (fileInfo != null)
        {
            using var stream = await _shellsFileStore.GetFileStreamAsync(OrchardCoreConstants.Shell.TenantsFileName);
            tenantsSettings = await JObject.LoadAsync(stream);
        }
        else
        {
            tenantsSettings = [];
        }

        var settings = tenantsSettings[tenant] as JsonObject ?? [];
        foreach (var key in data.Keys)
        {
            if (data[key] != null)
            {
                settings[key] = data[key];
            }
            else
            {
                settings.Remove(key);
            }
        }

        tenantsSettings[tenant] = settings;

        var tenantsSettingsString = tenantsSettings.ToJsonString(JOptions.Default);
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(tenantsSettingsString));

        await _shellsFileStore.CreateFileFromStreamAsync(OrchardCoreConstants.Shell.TenantsFileName, memoryStream);
    }

    public async Task RemoveAsync(string tenant)
    {
        var fileInfo = await _shellsFileStore.GetFileInfoAsync(OrchardCoreConstants.Shell.TenantsFileName);

        if (fileInfo != null)
        {
            JsonObject tenantsSettings;
            using (var stream = await _shellsFileStore.GetFileStreamAsync(OrchardCoreConstants.Shell.TenantsFileName))
            {
                tenantsSettings = await JObject.LoadAsync(stream);
            }

            tenantsSettings.Remove(tenant);

            var tenantsSettingsString = tenantsSettings.ToJsonString(JOptions.Default);
            using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(tenantsSettingsString));

            await _shellsFileStore.CreateFileFromStreamAsync(OrchardCoreConstants.Shell.TenantsFileName, memoryStream);
        }
    }

    private async Task<bool> TryMigrateFromFileAsync()
    {
        if (!File.Exists(_tenantsFileSystemName))
        {
            return false;
        }

        using var file = File.OpenRead(_tenantsFileSystemName);
        await _shellsFileStore.CreateFileFromStreamAsync(OrchardCoreConstants.Shell.TenantsFileName, file);

        return true;
    }
}
