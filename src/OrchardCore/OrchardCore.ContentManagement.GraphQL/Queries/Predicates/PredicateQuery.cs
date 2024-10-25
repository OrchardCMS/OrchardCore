using YesSql;

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

        var values = propertyPath.Split('.', 2);

        // if empty prefix, use default (empty alias)
        var aliasPath = values.Length == 1 ? string.Empty : values[0];

        // get the actual index from the alias
        if (_aliases.TryGetValue(aliasPath, out alias))
        {
            // get the index property provider fore the alias
            var propertyProvider = _propertyProviders.FirstOrDefault(x => x.IndexName.Equals(alias, StringComparison.OrdinalIgnoreCase));

            if (propertyProvider != null)
            {
                if (propertyProvider.TryGetValue(values.Last(), out var columnName))
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

        // No aliases registered for this path, return the formatted path.
        return;
    }

    public string GetColumnName(string propertyPath)
    {
        ArgumentNullException.ThrowIfNull(propertyPath);

        // Check if there's an alias for the full path
        // aliasPart.Alias -> AliasFieldIndex.Alias
        if (_aliases.TryGetValue(propertyPath, out var alias))
        {
            return Dialect.QuoteForColumnName(alias);
        }

        var values = propertyPath.Split('.', 2);

        // if empty prefix, use default (empty alias)
        var aliasPath = values.Length == 1 ? string.Empty : values[0];

        // get the actual index from the alias
        if (_aliases.TryGetValue(aliasPath, out alias))
        {
            var tableAlias = _tableAliases[alias];
            // get the index property provider fore the alias
            var propertyProvider = _propertyProviders.FirstOrDefault(x => x.IndexName.Equals(alias, StringComparison.OrdinalIgnoreCase));

            if (propertyProvider != null)
            {
                if (propertyProvider.TryGetValue(values.Last(), out var columnName))
                {
                    // Switch the given alias in the path with the mapped alias.
                    // aliasPart.alias -> AliasPartIndex.Alias
                    return $"{Dialect.QuoteForAliasName(tableAlias)}.{Dialect.QuoteForColumnName(columnName)}";
                }
            }
            else
            {
                // no property provider exists; hope sql is case-insensitive (will break postgres; property providers must be supplied for postgres)
                // Switch the given alias in the path with the mapped alias.
                // aliasPart.Alias -> AliasPartIndex.alias
                return $"{Dialect.QuoteForAliasName(tableAlias)}.{Dialect.QuoteForColumnName(values.Last())}";
            }
        }

        // No aliases registered for this path, return the formatted path.
        return Dialect.QuoteForColumnName(propertyPath);
    }

    public IEnumerable<string> GetUsedAliases()
    {
        return _usedAliases;
    }
}
