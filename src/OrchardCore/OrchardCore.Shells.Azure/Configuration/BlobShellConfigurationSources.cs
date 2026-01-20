using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Environment.Shell.Configuration.Internal;
using OrchardCore.FileStorage;
using OrchardCore.Shells.Azure.Services;

namespace OrchardCore.Shells.Azure.Configuration;

public class BlobShellConfigurationSources : IShellConfigurationSources
{
    private readonly IShellsFileStore _shellsFileStore;
    private readonly BlobShellStorageOptions _blobOptions;
    private readonly string _container;
    private readonly string _fileStoreContainer;

    public BlobShellConfigurationSources(
        IShellsFileStore shellsFileStore,
        BlobShellStorageOptions blobOptions,
        IOptions<ShellOptions> shellOptions)
    {
        _shellsFileStore = shellsFileStore;
        _blobOptions = blobOptions;

        // e.g., Sites.
        _container = shellOptions.Value.ShellsContainerName;

        // e.g., App_Data/Sites
        _fileStoreContainer = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, shellOptions.Value.ShellsContainerName);
    }

    public async Task AddSourcesAsync(string tenant, IConfigurationBuilder builder)
    {
        var appSettings = IFileStoreExtensions.Combine(null, _container, tenant, OrchardCoreConstants.Configuration.ApplicationSettingsFileName);
        var fileInfo = await _shellsFileStore.GetFileInfoAsync(appSettings);

        if (fileInfo == null && _blobOptions.MigrateFromFiles)
        {
            if (await TryMigrateFromFileAsync(tenant, appSettings))
            {
                fileInfo = await _shellsFileStore.GetFileInfoAsync(appSettings);
            }
        }

        if (fileInfo != null)
        {
            var stream = await _shellsFileStore.GetFileStreamAsync(appSettings);
            builder.AddTenantJsonStream(stream);
        }
    }

    public async Task SaveAsync(string tenant, IDictionary<string, string> data)
    {
        var appsettings = IFileStoreExtensions.Combine(null, _container, tenant, OrchardCoreConstants.Configuration.ApplicationSettingsFileName);

        var fileInfo = await _shellsFileStore.GetFileInfoAsync(appsettings);

#pragma warning disable CA1859 // Use concrete types when possible for improved performance
        IDictionary<string, string> configData;
#pragma warning restore CA1859 // Use concrete types when possible for improved performance
        if (fileInfo != null)
        {
            using var stream = await _shellsFileStore.GetFileStreamAsync(appsettings);
            configData = await JsonConfigurationParser.ParseAsync(stream);
        }
        else
        {
            configData = new Dictionary<string, string>();
        }

        foreach (var key in data.Keys)
        {
            if (data[key] is not null)
            {
                configData[key] = data[key];
            }
            else
            {
                configData.Remove(key);
            }
        }

        var configurationString = configData.ToJsonObject().ToJsonString(JOptions.Default);
        using var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(configurationString));

        await _shellsFileStore.CreateFileFromStreamAsync(appsettings, memoryStream);
    }

    public async Task RemoveAsync(string tenant)
    {
        var appSettings = IFileStoreExtensions.Combine(null, _container, tenant, OrchardCoreConstants.Configuration.ApplicationSettingsFileName);

        var fileInfo = await _shellsFileStore.GetFileInfoAsync(appSettings);
        if (fileInfo != null)
        {
            await _shellsFileStore.RemoveFileAsync(appSettings);
        }
    }

    private async Task<bool> TryMigrateFromFileAsync(string tenant, string destFile)
    {
        var tenantFile = Path.Combine(_fileStoreContainer, tenant, OrchardCoreConstants.Configuration.ApplicationSettingsFileName);
        if (!File.Exists(tenantFile))
        {
            return false;
        }

        using var file = File.OpenRead(tenantFile);
        await _shellsFileStore.CreateFileFromStreamAsync(destFile, file);

        return true;
    }
}
