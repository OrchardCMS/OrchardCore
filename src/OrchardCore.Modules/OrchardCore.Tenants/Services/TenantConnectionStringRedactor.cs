using System.Data.Common;

namespace OrchardCore.Tenants.Services;

public static class TenantConnectionStringRedactor
{
    private const string RedactedValue = "******";

    public static string RedactPassword(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return connectionString;
        }

        try
        {
            var builder = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString,
            };

            foreach (var key in builder.Keys.Cast<string>().ToArray())
            {
                if (IsPasswordKey(key))
                {
                    builder[key] = RedactedValue;
                }
            }

            return builder.ConnectionString;
        }
        catch (ArgumentException)
        {
            return connectionString;
        }
    }

    public static string RestoreIfRedacted(string currentConnectionString, string postedConnectionString)
    {
        if (string.IsNullOrEmpty(currentConnectionString) || string.IsNullOrEmpty(postedConnectionString))
        {
            return postedConnectionString;
        }

        return string.Equals(postedConnectionString, RedactPassword(currentConnectionString), StringComparison.Ordinal)
            ? currentConnectionString
            : postedConnectionString;
    }

    private static bool IsPasswordKey(string key) =>
        string.Equals(key, "Password", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(key, "Pwd", StringComparison.OrdinalIgnoreCase);
}
