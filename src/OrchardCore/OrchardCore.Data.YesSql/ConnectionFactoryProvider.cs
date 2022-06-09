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

        return providerName.ToLower() switch
        {
            "sqlconnection" => new DbConnectionFactory<SqlConnection>(connectionString),
            "sqlite" => new DbConnectionFactory<MySqlConnection>(connectionString),
            "mysql" => new DbConnectionFactory<SqliteConnection>(connectionString),
            "postgres" => new DbConnectionFactory<NpgsqlConnection>(connectionString),
            _ => throw new ArgumentOutOfRangeException("The provider '{0}' is not supported.", providerName)
        };
    }
}
