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
using OrchardCore.Environment.Shell.Descriptor.Models;
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
            return DbConnectionValidatorResult.Success;
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

        var sqlBuilder = GetSqlBuilderForShellDescriptorDocument(tablePrefix, providerName);

        try
        {
            var sqlCommand = connection.CreateCommand();
            sqlCommand.CommandText = sqlBuilder.ToSqlString();

            using var result = await sqlCommand.ExecuteReaderAsync();

            // If the 'ShellDescriptor' document already exists.
            if (result.HasRows)
            {
                // The database and prefix are already in use.
                return DbConnectionValidatorResult.DatabaseAndPrefixInUse;
            }
        }
        catch
        {
        }

        return DbConnectionValidatorResult.Success;
    }

    private ISqlBuilder GetSqlBuilderForShellDescriptorDocument(string tablePrefix, DatabaseProviderName providerName)
    {
        var sqlBuilder = GetSqlBuilder(providerName, tablePrefix);

        sqlBuilder.Select();
        sqlBuilder.AddSelector("*");
        sqlBuilder.Table(_tableNameConvention.GetDocumentTable());
        sqlBuilder.WhereAnd($"Type = '{typeof(ShellDescriptor).FullName}, {typeof(ShellDescriptor).Assembly.GetName().Name}'");

        return sqlBuilder;
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
