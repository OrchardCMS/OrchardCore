using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;

namespace OrchardCore.Data
{
    public class SqliteOptionsConfiguration : IConfigureOptions<SqliteOptions>
    {
        private static readonly bool _defaultUseConnectionPooling = true;
        private readonly IShellConfiguration _shellConfiguration;

        public SqliteOptionsConfiguration(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public void Configure(SqliteOptions options)
        {
            var section = _shellConfiguration.GetSection("OrchardCore_Data_Sqlite");

            options.UseConnectionPooling = section.GetValue(nameof(options.UseConnectionPooling), _defaultUseConnectionPooling);
        }
    }
}
