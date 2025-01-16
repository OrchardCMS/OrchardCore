using YesSql;
using YesSql.Provider.MySql;
using YesSql.Provider.PostgreSql;
using YesSql.Provider.Sqlite;
using YesSql.Provider.SqlServer;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates;

public class PredicateQuery : IPredicateQuery
{
    private readonly IEnumerable<IIndexPropertyProvider> _propertyProviders;

    private readonly HashSet<string> _usedAliases = [];
    private readonly Dictionary<string, string> _aliases = [];
    private readonly Dictionary<string, string> _tableAliases = [];

    public PredicateQuery(
        IConfiguration configuration,
        IEnumerable<IIndexPropertyProvider> propertyProviders)
    {
        Dialect = configuration.SqlDialect;
        _propertyProviders = propertyProviders;
    }

    public ISqlDialect Dialect { get; set; }

    public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

    public string NewQueryParameter(object value)
    {
        var count = Parameters.Count;
        var parameterName = $"@x{count + 1}";

        Parameters.Add(parameterName, value);

        return parameterName;
    }

    public void CreateAlias(string path, string alias)
    {
        ArgumentNullException.ThrowIfNull(path);

        ArgumentNullException.ThrowIfNull(alias);

        _aliases[path] = alias;
    }

    public void CreateTableAlias(string path, string tableAlias)
    {
        ArgumentNullException.ThrowIfNull(path);

        ArgumentNullException.ThrowIfNull(tableAlias);

        _tableAliases[path] = tableAlias;
    }

    public void SearchUsedAlias(string propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);

        // Check if there's an alias for the full path
        // aliasPart.Alias -> AliasFieldIndex.Alias
        if (_aliases.TryGetValue(propertyPath, out var alias))
        {
            _usedAliases.Add(alias);
            return;
        }

        var index = propertyPath.IndexOf('.');

        // if empty prefix, use default (empty alias)
        var aliasPath = index == -1 ? string.Empty : propertyPath[..index];

        // get the actual index from the alias
        if (_aliases.TryGetValue(aliasPath, out alias))
        {
            // get the index property provider fore the alias
            var propertyProvider = _propertyProviders.FirstOrDefault(x => x.IndexName.Equals(alias, StringComparison.OrdinalIgnoreCase));

            if (propertyProvider != null)
            {
                if (propertyProvider.TryGetValue(propertyPath[(index + 1)..], out var columnName))
                {
                    _usedAliases.Add(alias);
                    return;
                }
            }
            else
            {
                _usedAliases.Add(alias);
                return;
            }
        }
        // else: No aliases registered for this path, return the formatted path.
    }

    public string GetColumnName(string propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);

        // Check if there's an alias for the full path
        // aliasPart.Alias -> AliasFieldIndex.Alias
        if (_aliases.TryGetValue(propertyPath, out var alias))
        {
            return Quote(alias);
        }

        // If the path is already quoted, return it as-is.
        if (IsQuoted(propertyPath))
        {
            return propertyPath;
        }

        var index = propertyPath.IndexOf('.');

        // if empty prefix, use default (empty alias)
        var aliasPath = index == -1 ? string.Empty : propertyPath[..index];

        // get the actual index from the alias
        if (_aliases.TryGetValue(aliasPath, out alias))
        {
            if (!_tableAliases.TryGetValue(alias, out var tableAlias))
            {
                throw new InvalidOperationException($"Missing table alias for path {alias}.");
            }

            // get the index property provider fore the alias
            var propertyProvider = _propertyProviders.FirstOrDefault(x => x.IndexName.Equals(alias, StringComparison.OrdinalIgnoreCase));

            if (propertyProvider != null)
            {
                if (propertyProvider.TryGetValue(propertyPath[(index + 1)..], out var columnName))
                {
                    // Switch the given alias in the path with the mapped alias.
                    // aliasPart.alias -> AliasPartIndex.Alias
                    return Quote(tableAlias, columnName);
                }
            }
            else
            {
                // no property provider exists; hope sql is case-insensitive (will break postgres; property providers must be supplied for postgres)
                // Switch the given alias in the path with the mapped alias.
                // aliasPart.Alias -> AliasPartIndex.alias
                return Quote(tableAlias, propertyPath[(index + 1)..]);
            }
        }

        // No aliases registered for this path, return the formatted path.
        return Quote(propertyPath);
    }

    public IEnumerable<string> GetUsedAliases()
    {
        return _usedAliases;
    }

    private string Quote(string alias)
    {
        if (IsQuoted(alias))
        {
            return alias;
        }

        var index = alias.IndexOf('.');
        return index == -1
            ? Dialect.QuoteForColumnName(alias)
            : Quote(alias[..index], alias[(index + 1)..]);
    }

    private string Quote(string tableAlias, string columnName)
    {
        if (!IsQuoted(tableAlias))
        {
            tableAlias = Dialect.QuoteForAliasName(tableAlias);
        }

        if (!IsQuoted(columnName))
        {
            columnName = Dialect.QuoteForColumnName(columnName);
        }

        return $"{tableAlias}.{columnName}";
    }

    private bool IsQuoted(string value)
    {
        if (value.Length >= 2)
        {
            var (startQuote, endQuote) = GetQuoteChars(Dialect);
            return value[0] == startQuote && value[^1] == endQuote;
        }

        return false;
    }

    private static (char startQuote, char endQuote) GetQuoteChars(ISqlDialect dialect)
        => dialect switch
        {
            MySqlDialect => ('`', '`'),
            PostgreSqlDialect => ('"', '"'),
            SqliteDialect or
            SqlServerDialect or
            _ => ('[', ']')
        };
}
