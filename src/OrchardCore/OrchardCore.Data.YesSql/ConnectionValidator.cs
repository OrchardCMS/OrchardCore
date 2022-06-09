using System;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using YesSql;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;
using YesSql.Services;
using YesSql.Sql;

namespace OrchardCore.Data;

public class ConnectionValidator : IConnectionValidator
{
    public async Task<ConnectionValidatorResult> ValidateAsync(string providerName, string connectionString, string tablePrefix)
    {
        ArgumentNullException.ThrowIfNull(providerName);
        ArgumentNullException.ThrowIfNull(connectionString);

        var factory = GetFactory(providerName, connectionString);

        using var connection = factory.CreateConnection();

        try
        {
            await connection.OpenAsync();
        }
        catch (Exception)
        {
            return ConnectionValidatorResult.InvalidConnection;
        }

        var tableConvention = new DefaultTableNameConvention();
        var selectBuilder = GetSqlBuilder(providerName, tablePrefix);

        selectBuilder.Select();
        selectBuilder.AddSelector("*");
        selectBuilder.Table(tableConvention.GetDocumentTable());
        selectBuilder.Take("1");

        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = selectBuilder.ToSqlString();

            using var result = await selectCommand.ExecuteReaderAsync();

            // at this point the query succeeded and the table exists
            return ConnectionValidatorResult.ValidDocumentExists;
        }
        catch (Exception)
        {
            // at this point we know that the document table does not exist;

            return ConnectionValidatorResult.ValidDocumentDoesNotExists;
        }
    }

    private IConnectionFactory GetFactory(string providerName, string connectionString)
    {
        return providerName.ToLower() switch
        {
            "sqlconnection" => new DbConnectionFactory<SqlConnection>(connectionString),
            "mysql" => new DbConnectionFactory<MySqlConnection>(connectionString),
            "sqlite" => new DbConnectionFactory<SqliteConnection>(connectionString),
            "postgres" => new DbConnectionFactory<NpgsqlConnection>(connectionString),
            _ => throw new ArgumentOutOfRangeException("The provider '{0}' is not supported.", providerName)
        };
    }

    private ISqlBuilder GetSqlBuilder(string providerName, string tablePrefix)
    {
        ISqlDialect dialect = providerName.ToLower() switch
        {
            "sqlconnection" => new SqlServerDialect(),
            "mysql" => new MySqlDialect(),
            "sqlite" => new SqliteDialect(),
            "postgres" => new PostgreSqlDialect(),
            _ => throw new ArgumentOutOfRangeException("The provider '{0}' is not supported.", providerName)
        };

        return new SqlBuilder(DatabaseHelper.GetStandardPrefix(tablePrefix), dialect);
    }
}
