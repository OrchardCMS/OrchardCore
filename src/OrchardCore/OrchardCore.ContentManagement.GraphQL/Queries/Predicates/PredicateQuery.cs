using System;
using System.Collections.Generic;
using System.Linq;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    public class PredicateQuery : IPredicateQuery
    {
        private readonly IEnumerable<IIndexPropertyProvider> _propertyProviders;
        private readonly HashSet<string> _usedAliases = new HashSet<string>();
        private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>();
        private readonly string _tablePrefix;

        public PredicateQuery(ISqlDialect dialect, ShellSettings shellSettings, IEnumerable<IIndexPropertyProvider> propertyProviders)
        {
            Dialect = dialect;
            _propertyProviders = propertyProviders;
            var tablePrefix = shellSettings["TablePrefix"];
            _tablePrefix = string.IsNullOrEmpty(tablePrefix) ? String.Empty : $"{tablePrefix}_";
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
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (alias == null) throw new ArgumentNullException(nameof(alias));

            _aliases[path] = alias;
        }

        public string GetColumnName(string propertyPath)
        {
            if (propertyPath == null) throw new ArgumentNullException(nameof(propertyPath));

            // Check if there's an alias for the full path
            // aliasPart.Alias -> AliasFieldIndex.Alias
            if (_aliases.TryGetValue(propertyPath, out string alias))
            {
                _usedAliases.Add(alias);
                return Dialect.QuoteForColumnName(alias);
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
                        // Switch the given alias in the path with the mapped alias.
                        // aliasPart.alias -> AliasPartIndex.Alias
                        return Dialect.QuoteForTableName($"{_tablePrefix}{alias}") + "." + Dialect.QuoteForColumnName(columnName);
                    }
                }
                else
                {
                    // no property provider exists; hope sql is case-insensitive (will break postgres; property providers must be supplied for postgres)
                    // Switch the given alias in the path with the mapped alias.
                    // aliasPart.Alias -> AliasPartIndex.alias
                    _usedAliases.Add(alias);
                    return Dialect.QuoteForTableName($"{_tablePrefix}{alias}") + "." + Dialect.QuoteForColumnName(values[1]);
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
}
