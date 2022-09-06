using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using OrchardCore.Data.YesSql.Abstractions;
using YesSql;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;
using YesSql.Sql;

namespace OrchardCore.Data;

public class DbConnectionValidator : IDbConnectionValidator
{
    private readonly IEnumerable<DatabaseProvider> _databaseProviders;
    private readonly ITableNameConvention _tableNameConvention;
    private readonly YesSqlOptions _yesSqlOptions;

    public DbConnectionValidator(
        IEnumerable<DatabaseProvider> databaseProviders,
        ITableNameConvention tableNameConvention,
        IOptions<YesSqlOptions> yesSqlOptions
        )
    {
        _databaseProviders = databaseProviders;
        _tableNameConvention = tableNameConvention;
        _yesSqlOptions = yesSqlOptions.Value;
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
            return DbConnectionValidatorResult.DocumentTableNotFound;
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

            // At this point, the query work and the 'Document' table exists.
            if (result.HasRows)
            {
                // At this point we know that the Document table and the ShellDescriptor record exists.
                return DbConnectionValidatorResult.ShellDescriptorDocumentFound;
            }

            // At this point the Document table exists with no ShellDescriptor document.
            return DbConnectionValidatorResult.DocumentTableFound;
        }
        catch
        {
            // At this point we know that the document table does not exist.
            return DbConnectionValidatorResult.DocumentTableNotFound;
        }
    }

    private ISqlBuilder GetSelectBuilderForDocumentTable(string tablePrefix, DatabaseProviderName providerName)
    {
        var selectBuilder = GetSqlBuilder(providerName, tablePrefix);
        selectBuilder.Select();
        selectBuilder.AddSelector("*");
        selectBuilder.Table(_tableNameConvention.GetDocumentTable());
        selectBuilder.Take("1");
        selectBuilder.WhereAnd("Type = 'OrchardCore.Shells.Database.Models.DatabaseShellsSettings, OrchardCore.Infrastructure'");

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
            _ => throw new ArgumentOutOfRangeException(nameof(providerName), "Unsupported Database Provider"),
        };
    }

    private ISqlBuilder GetSqlBuilder(DatabaseProviderName providerName, string tablePrefix)
    {
        ISqlDialect dialect = providerName switch
        {
            DatabaseProviderName.SqlConnection => new SqlServerDialect(),
            DatabaseProviderName.MySql => new MySqlDialect(),
            DatabaseProviderName.Sqlite => new SqliteDialect(),
            DatabaseProviderName.Postgres => new PostgreSqlDialect(),
            _ => throw new ArgumentOutOfRangeException(nameof(providerName), "Unsupported Database Provider"),
        };

        var prefix = String.Empty;

        if (!String.IsNullOrEmpty(tablePrefix))
        {
            prefix = tablePrefix.Trim() + (_yesSqlOptions.TablePrefixSeparator ?? String.Empty);
        }

        return new SqlBuilder(prefix, dialect);
    }
}
