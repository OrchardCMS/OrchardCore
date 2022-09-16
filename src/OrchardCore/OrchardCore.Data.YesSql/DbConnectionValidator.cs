using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Npgsql;
using OrchardCore.Data.YesSql;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Descriptor.Models;
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
    private static readonly string[] _requiredDocumentTableColumns = new[] { "Id", "Type", "Content", "Version" };
    private static readonly string _shellDescriptorTypeColumnValue = new TypeService()[typeof(ShellDescriptor)];

    private readonly IEnumerable<DatabaseProvider> _databaseProviders;
    private readonly YesSqlOptions _yesSqlOptions;
    private readonly SqliteOptions _sqliteOptions;
    private readonly ShellOptions _shellOptions;

    public DbConnectionValidator(
        IEnumerable<DatabaseProvider> databaseProviders,
        IOptions<YesSqlOptions> yesSqlOptions,
        IOptions<SqliteOptions> sqliteOptions,
        IOptions<ShellOptions> shellOptions)
    {
        _databaseProviders = databaseProviders;
        _yesSqlOptions = yesSqlOptions.Value;
        _sqliteOptions = sqliteOptions.Value;
        _shellOptions = shellOptions.Value;
    }

    public async Task<DbConnectionValidatorResult> ValidateAsync(string databaseProvider, string connectionString, string tablePrefix, string shellName)
    {
        if (String.IsNullOrWhiteSpace(databaseProvider))
        {
            return DbConnectionValidatorResult.NoProvider;
        }

        var provider = _databaseProviders.FirstOrDefault(x => x.Value == databaseProvider);
        if (provider == null)
        {
            return DbConnectionValidatorResult.UnsupportedProvider;
        }

        if (!provider.HasConnectionString)
        {
            if (provider.Value != DatabaseProviderValue.Sqlite)
            {
                return DbConnectionValidatorResult.DocumentTableNotFound;
            }

            connectionString = SqliteHelper.GetConnectionString(_sqliteOptions, _shellOptions, shellName);
        }

        if (String.IsNullOrWhiteSpace(connectionString))
        {
            return DbConnectionValidatorResult.InvalidConnection;
        }

        var factory = GetFactory(databaseProvider, connectionString);

        using var connection = factory.CreateConnection();

        try
        {
            await connection.OpenAsync();
        }
        catch
        {
            if (provider.Value != DatabaseProviderValue.Sqlite)
            {
                return DbConnectionValidatorResult.InvalidConnection;
            }

            return DbConnectionValidatorResult.DocumentTableNotFound;
        }

        var selectBuilder = GetSelectBuilderForDocumentTable(tablePrefix, databaseProvider);
        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = selectBuilder.ToSqlString();

            using var result = await selectCommand.ExecuteReaderAsync();
            if (shellName != ShellHelper.DefaultShellName)
            {
                // The 'Document' table exists.
                return DbConnectionValidatorResult.DocumentTableFound;
            }

            var requiredColumnsCount = Enumerable.Range(0, result.FieldCount)
                .Select(result.GetName)
                .Where(c => _requiredDocumentTableColumns.Contains(c, StringComparer.OrdinalIgnoreCase))
                .Count();

            if (requiredColumnsCount != _requiredDocumentTableColumns.Length)
            {
                // The 'Document' table exists with another schema.
                return DbConnectionValidatorResult.DocumentTableFound;
            }
        }
        catch
        {
            // The 'Document' table does not exist.
            return DbConnectionValidatorResult.DocumentTableNotFound;
        }

        selectBuilder = GetSelectBuilderForShellDescriptorDocument(tablePrefix, databaseProvider);
        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = selectBuilder.ToSqlString();

            using var result = await selectCommand.ExecuteReaderAsync();
            if (!result.HasRows)
            {
                // The 'Document' table exists with no 'ShellDescriptor' document.
                return DbConnectionValidatorResult.ShellDescriptorDocumentNotFound;
            }
        }
        catch
        {
        }

        // The 'Document' table exists.
        return DbConnectionValidatorResult.DocumentTableFound;
    }

    private ISqlBuilder GetSelectBuilderForDocumentTable(string tablePrefix, string databaseProvider)
    {
        var selectBuilder = GetSqlBuilder(databaseProvider, tablePrefix);

        selectBuilder.Select();
        selectBuilder.Selector("*");
        selectBuilder.Table(_yesSqlOptions.TableNameConvention.GetDocumentTable());
        selectBuilder.Take("1");

        return selectBuilder;
    }

    private ISqlBuilder GetSelectBuilderForShellDescriptorDocument(string tablePrefix, string databaseProvider)
    {
        var selectBuilder = GetSqlBuilder(databaseProvider, tablePrefix);

        selectBuilder.Select();
        selectBuilder.Selector("*");
        selectBuilder.Table(_yesSqlOptions.TableNameConvention.GetDocumentTable());
        selectBuilder.WhereAnd($"Type = '{_shellDescriptorTypeColumnValue}'");
        selectBuilder.Take("1");

        return selectBuilder;
    }

    private static IConnectionFactory GetFactory(string databaseProvider, string connectionString)
    {
        return databaseProvider switch
        {
            DatabaseProviderValue.SqlConnection => new DbConnectionFactory<SqlConnection>(connectionString),
            DatabaseProviderValue.MySql => new DbConnectionFactory<MySqlConnection>(connectionString),
            DatabaseProviderValue.Sqlite => new DbConnectionFactory<SqliteConnection>(connectionString),
            DatabaseProviderValue.Postgres => new DbConnectionFactory<NpgsqlConnection>(connectionString),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), "Unsupported database provider"),
        };
    }

    private ISqlBuilder GetSqlBuilder(string databaseProvider, string tablePrefix)
    {
        ISqlDialect dialect = databaseProvider switch
        {
            DatabaseProviderValue.SqlConnection => new SqlServerDialect(),
            DatabaseProviderValue.MySql => new MySqlDialect(),
            DatabaseProviderValue.Sqlite => new SqliteDialect(),
            DatabaseProviderValue.Postgres => new PostgreSqlDialect(),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), "Unsupported database provider"),
        };

        var prefix = String.Empty;
        if (!String.IsNullOrWhiteSpace(tablePrefix))
        {
            prefix = tablePrefix.Trim() + _yesSqlOptions.TablePrefixSeparator;
        }

        return new SqlBuilder(prefix, dialect);
    }
}
