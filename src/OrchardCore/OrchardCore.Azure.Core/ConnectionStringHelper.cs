namespace OrchardCore.Azure.Core;

public static class ConnectionStringHelper
{
    public static string Extract(string connectionString, string valueKey)
    {
        ArgumentNullException.ThrowIfNull(connectionString);
        ArgumentNullException.ThrowIfNull(valueKey);

        var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length == 2 && kv[0].Trim().Equals(valueKey, StringComparison.OrdinalIgnoreCase))
            {
                return kv[1].Trim();
            }
        }

        return null;
    }
}
