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

        var (factory, dialect) = GetConnectionFactoryAndSqlDialect(databaseProvider, connectionString);

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

        var selectBuilder = GetDocumentTableSelectBuilder(
            tablePrefix,
            _yesSqlOptions.TablePrefixSeparator,
            _yesSqlOptions.TableNameConvention.GetDocumentTable(),
            dialect);
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

        selectBuilder = GetDocumentTableSelectBuilder(
            tablePrefix,
            _yesSqlOptions.TablePrefixSeparator,
            _yesSqlOptions.TableNameConvention.GetDocumentTable(),
            dialect,
            isShellDescriptorDocument: true);

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

    private static (IConnectionFactory, ISqlDialect) GetConnectionFactoryAndSqlDialect(string providerName, string connectionString)
    {
        IConnectionFactory factory;
        ISqlDialect dialect;
        switch (providerName)
        {
            case DatabaseProviderValue.SqlConnection:
                factory = new DbConnectionFactory<SqlConnection>(connectionString);
                dialect = new SqlServerDialect();
                break;
            case DatabaseProviderValue.Sqlite:
                factory = new DbConnectionFactory<SqliteConnection>(connectionString);
                dialect = new SqliteDialect();
                break;
            case DatabaseProviderValue.MySql:
                factory = new DbConnectionFactory<MySqlConnection>(connectionString);
                dialect = new MySqlDialect();
                break;
            case DatabaseProviderValue.Postgres:
                factory = new DbConnectionFactory<NpgsqlConnection>(connectionString);
                dialect = new PostgreSqlDialect();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(providerName), "Unsupported Database Provider");
        }

        return (factory, dialect);
    }

    private static ISqlBuilder GetDocumentTableSelectBuilder(string tablePrefix, string tablePrefixSeparator, string documentTable, ISqlDialect dialect, bool isShellDescriptorDocument = false)
    {
        var prefix = String.Empty;
        if (!String.IsNullOrEmpty(tablePrefix))
        {
            prefix = tablePrefix.Trim() + (tablePrefixSeparator ?? String.Empty);
        }

        var selectBuilder = new SqlBuilder(prefix, dialect);
        selectBuilder.Select();
        selectBuilder.Selector("*");
        selectBuilder.Table(documentTable, alias: null, schema: null);

        if (isShellDescriptorDocument)
        {
            selectBuilder.WhereAnd($"Type = '{_shellDescriptorTypeColumnValue}'");
        }

        selectBuilder.Take("1");

        return selectBuilder;
    }
}
