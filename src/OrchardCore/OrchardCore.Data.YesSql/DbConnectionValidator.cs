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
using YesSql.Services;
using YesSql.Sql;

namespace OrchardCore.Data;

public class DbConnectionValidator : IDbConnectionValidator
{
    private static readonly string[] _requiredDocumentTableColumns = new[] { "Id", "Type", "Content", "Version" };
    private static readonly string _shellDescriptorTypeColumnValue = new TypeService()[typeof(ShellDescriptor)];

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

    public async Task<DbConnectionValidatorResult> ValidateAsync(string databaseProvider, string connectionString, string tablePrefix, bool isDefaultShell)
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

        var (factory, dialect) = GetConnectionFactoryAndSqlDialect(providerName, connectionString);

        using var connection = factory.CreateConnection();

        try
        {
            await connection.OpenAsync();
        }
        catch
        {
            return DbConnectionValidatorResult.InvalidConnection;
        }

        var selectBuilder = GetDocumentTableSelectBuilder(
            tablePrefix,
            _yesSqlOptions.TablePrefixSeparator,
            _tableNameConvention.GetDocumentTable(),
            dialect);
        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = selectBuilder.ToSqlString();

            using var result = await selectCommand.ExecuteReaderAsync();
            if (!isDefaultShell)
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

        selectBuilder = GetDocumentTableSelectBuilder(tablePrefix,
            _yesSqlOptions.TablePrefixSeparator,
            _tableNameConvention.GetDocumentTable(),
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

    private static (IConnectionFactory, ISqlDialect) GetConnectionFactoryAndSqlDialect(DatabaseProviderName providerName, string connectionString)
    {
        IConnectionFactory factory;
        ISqlDialect dialect;
        switch (providerName)
        {
            case DatabaseProviderName.SqlConnection:
                factory = new DbConnectionFactory<SqlConnection>(connectionString);
                dialect = new SqlServerDialect();
                break;
            case DatabaseProviderName.Sqlite:
                factory = new DbConnectionFactory<SqliteConnection>(connectionString);
                dialect = new SqliteDialect();
                break;
            case DatabaseProviderName.MySql:
                factory = new DbConnectionFactory<MySqlConnection>(connectionString);
                dialect = new MySqlDialect();
                break;
            case DatabaseProviderName.Postgres:
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
        selectBuilder.Table(documentTable);

        if (isShellDescriptorDocument)
        {
            selectBuilder.WhereAnd($"Type = '{_shellDescriptorTypeColumnValue}'");
        }

        selectBuilder.Take("1");

        return selectBuilder;
    }
}
