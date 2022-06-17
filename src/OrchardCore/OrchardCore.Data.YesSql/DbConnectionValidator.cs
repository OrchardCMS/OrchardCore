using System;
using System.Collections.Generic;
using System.Linq;
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

public class DbConnectionValidator : IDbConnectionValidator
{
    private readonly IEnumerable<DatabaseProvider> _databaseProviders;

    public DbConnectionValidator(IEnumerable<DatabaseProvider> databaseProviders)
    {
        _databaseProviders = databaseProviders;
    }

    public async Task<DbConnectionValidatorResult> ValidateAsync(string databaseProvider, string connectionString, string tablePrefix)
    {
        if (String.IsNullOrWhiteSpace(databaseProvider))
        {
            return DbConnectionValidatorResult.NoProvider;
        }

        if (!Enum.TryParse(databaseProvider, out DatabaseProviderName providerName) || providerName == DatabaseProviderName.None)
        {
            return DbConnectionValidatorResult.UnsupportedProvider;
        }

        var provider = _databaseProviders.FirstOrDefault(x => x.Value == providerName);

        if (provider != null && !provider.HasConnectionString)
        {
            return DbConnectionValidatorResult.DocumentNotFound;
        }

        if (String.IsNullOrWhiteSpace(connectionString))
        {
            return DbConnectionValidatorResult.InvalidConnection;
        }

        var factory = GetFactory(providerName, connectionString);

        using var connection = factory.CreateConnection();

        try
        {
            await connection.OpenAsync();
        }
        catch
        {
            return DbConnectionValidatorResult.InvalidConnection;
        }

        var selectBuilder = GetSelectBuilderForDocumentTable(tablePrefix, providerName);

        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = selectBuilder.ToSqlString();

            using var result = await selectCommand.ExecuteReaderAsync();

            // at this point the query succeeded and the table exists
            return DbConnectionValidatorResult.DocumentFound;
        }
        catch
        {
            // at this point we know that the document table does not exist

            return DbConnectionValidatorResult.DocumentNotFound;
        }
    }

    private static ISqlBuilder GetSelectBuilderForDocumentTable(string tablePrefix, DatabaseProviderName providerName)
    {
        var tableConvention = new DefaultTableNameConvention();
        var selectBuilder = GetSqlBuilder(providerName, tablePrefix);

        selectBuilder.Select();
        selectBuilder.AddSelector("*");
        selectBuilder.Table(tableConvention.GetDocumentTable());
        selectBuilder.Take("1");

        return selectBuilder;
    }

    private static IConnectionFactory GetFactory(DatabaseProviderName providerName, string connectionString)
    {
        return providerName switch
        {
            DatabaseProviderName.SqlConnection => new DbConnectionFactory<SqlConnection>(connectionString),
            DatabaseProviderName.MySql => new DbConnectionFactory<MySqlConnection>(connectionString),
            DatabaseProviderName.Sqlite => new DbConnectionFactory<SqliteConnection>(connectionString),
            DatabaseProviderName.Postgres => new DbConnectionFactory<NpgsqlConnection>(connectionString),
            _ => throw new ArgumentOutOfRangeException("Unsupported Database Provider"),
        };
    }

    private static ISqlBuilder GetSqlBuilder(DatabaseProviderName providerName, string tablePrefix)
    {
        ISqlDialect dialect = providerName switch
        {
            DatabaseProviderName.SqlConnection => new SqlServerDialect(),
            DatabaseProviderName.MySql => new MySqlDialect(),
            DatabaseProviderName.Sqlite => new SqliteDialect(),
            DatabaseProviderName.Postgres => new PostgreSqlDialect(),
            _ => throw new ArgumentOutOfRangeException("Unsupported Database Provider"),
        };

        return new SqlBuilder(DatabaseHelper.GetStandardPrefix(tablePrefix), dialect);
    }
}
