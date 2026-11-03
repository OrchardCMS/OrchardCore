using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Data;

public sealed class SqliteOptionsConfiguration : IConfigureOptions<SqliteOptions>
{
    private readonly IShellConfiguration _shellConfiguration;

    public SqliteOptionsConfiguration(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void Configure(SqliteOptions options)
    {
        // The 'OrchardCore_Data_Sqlite' section is deprecated and will be removed in a future major version, use 'Data:Sqlite' instead.
        _shellConfiguration.GetSectionCompat("Data:Sqlite", "OrchardCore_Data_Sqlite").Bind(options);
    }
}
