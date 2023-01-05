using System;
using System.Collections.Generic;
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

        var (factory, sqlDialect) = GetFactoryAndSqlDialect(context.DatabaseProvider, connectionString);

        using var connection = factory.CreateConnection();

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

        var sqlBuilder = GetSqlBuilder(sqlDialect, context.TablePrefix, context.TableOptions.TableNameSeparator);

        try
        {
            var selectCommand = connection.CreateCommand();
            selectCommand.CommandText = GetDocumentCommandText(sqlBuilder, documentName, context.Schema);

            using var result = await selectCommand.ExecuteReaderAsync();
            if (context.ShellName != ShellHelper.DefaultShellName)
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
            selectCommand.CommandText = GetDocumentCommandText(sqlBuilder, documentName, context.Schema, isShellDescriptorDocument: true);

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

    private static string GetDocumentCommandText(ISqlBuilder sqlBuilder, string documentTable, string schema, bool isShellDescriptorDocument = false)
    {
        sqlBuilder.Select();
        sqlBuilder.Selector("*");
        sqlBuilder.Table(documentTable, alias: null, schema);

        if (isShellDescriptorDocument)
        {
            sqlBuilder.WhereAnd($"Type = '{_shellDescriptorTypeColumnValue}'");
        }

        sqlBuilder.Take("1");

        return sqlBuilder.ToString();
    }

    private static (IConnectionFactory connectionFactory, ISqlDialect sqlDialect) GetFactoryAndSqlDialect(
        string databaseProvider,
        string connectionString) => databaseProvider switch
        {
            DatabaseProviderValue.SqlConnection => (new DbConnectionFactory<SqlConnection>(connectionString), new SqlServerDialect()),
            DatabaseProviderValue.MySql => (new DbConnectionFactory<MySqlConnection>(connectionString), new MySqlDialect()),
            DatabaseProviderValue.Sqlite => (new DbConnectionFactory<SqliteConnection>(connectionString), new SqliteDialect()),
            DatabaseProviderValue.Postgres => (new DbConnectionFactory<NpgsqlConnection>(connectionString), new PostgreSqlDialect()),
            _ => throw new ArgumentOutOfRangeException(nameof(databaseProvider), "Unsupported database provider"),
        };

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
