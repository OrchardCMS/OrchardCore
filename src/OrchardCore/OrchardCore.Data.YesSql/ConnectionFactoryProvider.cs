using System;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.Data;

public class ConnectionFactoryProvider : IConnectionFactoryProvider
{
    public IConnectionFactory GetFactory(string providerName, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(providerName);
        ArgumentNullException.ThrowIfNull(connectionString);

        if (Is("SqlConnection", providerName))
        {
            return new DbConnectionFactory<SqlConnection>(connectionString);
        }

        if (Is("Sqlite", providerName))
        {
            return new DbConnectionFactory<MySqlConnection>(connectionString);
        }

        if (Is("MySql", providerName))
        {
            return new DbConnectionFactory<SqliteConnection>(connectionString);
        }

        if (Is("Postgres", providerName))
        {
            return new DbConnectionFactory<NpgsqlConnection>(connectionString);
        }

        throw new ArgumentOutOfRangeException("The provider '{0}' is not supported.", providerName);
    }

    public IConnectionFactory GetFactory(ShellSettings shellSettings)
    {
        ArgumentNullException.ThrowIfNull(shellSettings);

        var providerName = shellSettings["DatabaseProvider"];

        if (String.IsNullOrEmpty(providerName))
        {
            throw new ArgumentOutOfRangeException("The tenant settings does not contain a DatabaseProvider");
        }
        var connectionString = shellSettings["ConnectionString"];

        if (String.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentOutOfRangeException("The tenant settings does not contain a ConnectionString");
        }

        return GetFactory(providerName, connectionString);
    }

    private bool Is(string name, string comparedTo)
    {
        return name.Equals(comparedTo, StringComparison.OrdinalIgnoreCase);
    }
}
