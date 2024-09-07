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
        var section = _shellConfiguration.GetSection("OrchardCore_Data_Sqlite");

        section.Bind(options);
    }
}
