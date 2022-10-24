using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Data;

public class ShellDatabaseTableOptionsConfiguration : IConfigureOptions<DatabaseTableOptions>
{
    private readonly ShellSettings _shellSettings;

    public ShellDatabaseTableOptionsConfiguration(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public void Configure(DatabaseTableOptions options)
    {
        DatabaseTableOptions.Configure(_shellSettings, options);
    }
}
