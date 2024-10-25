using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Shells.Database.Extensions;
using OrchardCore.Shells.Database.Models;
using YesSql;

namespace OrchardCore.Shells.Database.Configuration;

public class DatabaseShellsSettingsSources : IShellsSettingsSources
{
    private readonly DatabaseShellsStorageOptions _options;
    private readonly IShellContextFactory _shellContextFactory;
    private readonly string _tenants;

    public DatabaseShellsSettingsSources(
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        IShellContextFactory shellContextFactory,
        IOptions<ShellOptions> shellOptions)

    {
        _options = configuration
            .GetSection("OrchardCore")
            .GetSectionCompat("OrchardCore_Shells_Database")
            .Get<DatabaseShellsStorageOptions>()
            ?? new DatabaseShellsStorageOptions();

        _shellContextFactory = shellContextFactory;

        _tenants = Path.Combine(shellOptions.Value.ShellsApplicationDataPath, OrchardCoreConstants.Shell.TenantsFileName);
    }

    public async Task AddSourcesAsync(IConfigurationBuilder builder)
    {
        var document = await GetDocumentAsync();
        if (document.ShellsSettings is not null)
        {
            var shellsSettingsString = document.ShellsSettings.ToJsonString(JOptions.Default);
            builder.AddTenantJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(shellsSettingsString)));
        }
    }

    public async Task AddSourcesAsync(string tenant, IConfigurationBuilder builder)
    {
        var document = await GetDocumentAsync();
        if (document.ShellsSettings is not null && document.ShellsSettings.ContainsKey(tenant))
        {
            var shellSettings = new JsonObject { [tenant] = document.ShellsSettings[tenant] };
            var shellSettingsString = shellSettings.ToJsonString(JOptions.Default);
            builder.AddTenantJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(shellSettingsString)));
        }
    }

    public async Task SaveAsync(string tenant, IDictionary<string, string> data)
    {
        await using var context = await _shellContextFactory.GetDatabaseContextAsync(_options);
        await (await context.CreateScopeAsync()).UsingServiceScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            var document = await session.Query<DatabaseShellsSettings>().FirstOrDefaultAsync();

            JsonObject tenantsSettings;
            if (document is not null)
            {
                tenantsSettings = document.ShellsSettings;
            }
            else
            {
                document = new DatabaseShellsSettings();
                tenantsSettings = [];
            }

            var settings = tenantsSettings[tenant] as JsonObject ?? [];

            foreach (var key in data.Keys)
            {
                if (data[key] is not null)
                {
                    settings[key] = data[key];
                }
                else
                {
                    settings.Remove(key);
                }
            }

            tenantsSettings[tenant] = settings;
            document.ShellsSettings = tenantsSettings;

            await session.SaveAsync(document, checkConcurrency: true);
        });
    }

    public async Task RemoveAsync(string tenant)
    {
        await using var context = await _shellContextFactory.GetDatabaseContextAsync(_options);
        await (await context.CreateScopeAsync()).UsingServiceScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            var document = await session.Query<DatabaseShellsSettings>().FirstOrDefaultAsync();
            if (document is not null)
            {
                document.ShellsSettings.Remove(tenant);
                await session.SaveAsync(document, checkConcurrency: true);
            }
        });
    }

    private async Task<DatabaseShellsSettings> GetDocumentAsync()
    {
        DatabaseShellsSettings document = null;

        await using var context = await _shellContextFactory.GetDatabaseContextAsync(_options);
        await (await context.CreateScopeAsync()).UsingServiceScopeAsync(async scope =>
        {
            var session = scope.ServiceProvider.GetRequiredService<ISession>();

            document = await session.Query<DatabaseShellsSettings>().FirstOrDefaultAsync();
            if (document is null)
            {
                document = new DatabaseShellsSettings();
                if (!_options.MigrateFromFiles || !await TryMigrateFromFileAsync(document))
                {
                    return;
                }

                await session.SaveAsync(document, checkConcurrency: true);
            }
        });

        return document;
    }

    private async Task<bool> TryMigrateFromFileAsync(DatabaseShellsSettings document)
    {
        if (!File.Exists(_tenants))
        {
            return false;
        }

        using var fileStream = File.OpenRead(_tenants);
        document.ShellsSettings = await JObject.LoadAsync(fileStream);

        return true;
    }
}
