using System.Data.Common;

namespace OrchardCore.Azure.Core;

public static class ConnectionStringHelper
{
    public static string Extract(string connectionString, string valueKey)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(valueKey);

        var builder = new DbConnectionStringBuilder
        {
            ConnectionString = connectionString,
        };

        if (builder.TryGetValue(valueKey, out var value))
        {
            return value?.ToString();
        }

        return null;
    }
}
