using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
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
    private readonly ITableNameConventionFactory _tableNameConventionFactory;
    private readonly ILogger _logger;
    private readonly SqliteOptions _sqliteOptions;
    private readonly ShellOptions _shellOptions;

    public DbConnectionValidator(
        IEnumerable<DatabaseProvider> databaseProviders,
        IOptions<SqliteOptions> sqliteOptions,
        IOptions<ShellOptions> shellOptions,
        ITableNameConventionFactory tableNameConventionFactory,
        ILogger<DbConnectionValidator> logger)
    {
        _databaseProviders = databaseProviders;
        _tableNameConventionFactory = tableNameConventionFactory;
        _logger = logger;
        _sqliteOptions = sqliteOptions.Value;
        _shellOptions = shellOptions.Value;
    }

    public async Task<DbConnectionValidatorResult> ValidateAsync(DbConnectionValidatorContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (String.IsNullOrWhiteSpace(context.DatabaseProvider))
        {
            return DbConnectionValidatorResult.NoProvider;
        }

        var provider = _databaseProviders.FirstOrDefault(provider => provider.Value == context.DatabaseProvider);
        if (provider == null)
        {
            return DbConnectionValidatorResult.UnsupportedProvider;
        }

        var connectionString = context.ConnectionString;
        if (!provider.HasConnectionString)
        {
            if (provider.Value != DatabaseProviderValue.Sqlite)
            {
                return DbConnectionValidatorResult.DocumentTableNotFound;
            }

            connectionString = SqliteHelper.GetConnectionString(_sqliteOptions, _shellOptions, context.ShellName);
        }

        if (String.IsNullOrWhiteSpace(connectionString))
        {
            return DbConnectionValidatorResult.InvalidConnection;
        }

        var factory = GetFactory(context.DatabaseProvider, connectionString);

        using var connection = factory.CreateConnection();

        // Prevent from creating an empty locked 'Sqlite' file.
        if (provider.Value == DatabaseProviderValue.Sqlite &&
            connection is SqliteConnection sqliteConnection &&
            !File.Exists(sqliteConnection.DataSource))
        {
            return DbConnectionValidatorResult.DocumentTableNotFound;
        }

        try
        {
            await connection.OpenAsync();
        }
        catch (Exception ex)
        {
            if (provider.Value != DatabaseProviderValue.Sqlite)
            {
                _logger.LogWarning(ex, "Unable to validate connection string.");

                return DbConnectionValidatorResult.InvalidConnection;
            }

            return DbConnectionValidatorResult.DocumentTableNotFound;
        }

        var tableNameConvention = _tableNameConventionFactory.Create(context.TableOptions);
        var documentName = tableNameConvention.GetDocumentTable();

        var sqlDialect = GetSqlDialect(context.DatabaseProvider);
        var sqlBuilder = GetSqlBuilder(sqlDialect, context.TablePrefix, context.TableOptions.TableNameSeparator);

        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = GetSelectBuilderForDocumentTable(sqlBuilder, documentName, context.Schema).ToSqlString();

            using var result = await selectCommand.ExecuteReaderAsync();
            if (!context.ShellName.IsDefaultShellName())
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

        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = GetSelectBuilderForShellDescriptorDocument(sqlBuilder, documentName, context.Schema).ToSqlString();

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

    private static ISqlBuilder GetSelectBuilderForDocumentTable(ISqlBuilder sqlBuilder, string documentTable, string schema)
    {
        sqlBuilder.Select();
        sqlBuilder.Selector("*");
        sqlBuilder.Table(documentTable, alias: null, schema);
        sqlBuilder.Take("1");

        return sqlBuilder;
    }

    private static ISqlBuilder GetSelectBuilderForShellDescriptorDocument(ISqlBuilder sqlBuilder, string documentTable, string schema)
    {
        sqlBuilder.Select();
        sqlBuilder.Selector("*");
        sqlBuilder.Table(documentTable, alias: null, schema);
        sqlBuilder.WhereAnd($"Type = '{_shellDescriptorTypeColumnValue}'");
        sqlBuilder.Take("1");

        return sqlBuilder;
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

    private static ISqlDialect GetSqlDialect(string databaseProvider)
    {
        return databaseProvider switch
        {
            DatabaseProviderValue.SqlConnection => new SqlServerDialect(),
            DatabaseProviderValue.MySql => new MySqlDialect(),
            DatabaseProviderValue.Sqlite => new SqliteDialect(),
            DatabaseProviderValue.Postgres => new PostgreSqlDialect(),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), "Unsupported database provider"),
        };
    }

    private static ISqlBuilder GetSqlBuilder(ISqlDialect sqlDialect, string tablePrefix, string tableNameSeparator)
    {
        var prefix = String.Empty;
        if (!String.IsNullOrWhiteSpace(tablePrefix))
        {
            prefix = tablePrefix.Trim() + tableNameSeparator;
        }

        return new SqlBuilder(prefix, sqlDialect);
    }
}
