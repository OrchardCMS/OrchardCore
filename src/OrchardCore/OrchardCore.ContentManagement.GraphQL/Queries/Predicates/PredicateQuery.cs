using System;
using System.Collections.Generic;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    public class PredicateQuery : IPredicateQuery
    {
		private readonly HashSet<string> _usedAliases = new HashSet<string>();
        private readonly IDictionary<string, string> _aliases = new Dictionary<string, string>();

        public PredicateQuery(ISqlDialect dialect)
        {
            Dialect = dialect;
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

            var values = propertyPath.Split(new []{'.'}, 2);
            if (values.Length == 1)
            {
                if (_aliases.TryGetValue(string.Empty, out alias))
                {
                    // Return the default alias
                    // ContentItemId -> ContentItemIndex.ContentItemId
                    _usedAliases.Add(alias);
                    return Dialect.QuoteForTableName(alias) + "." + Dialect.QuoteForColumnName(values[0]);
                }
            }
            else
            {
                if (_aliases.TryGetValue(values[0], out alias))
                {
                    // Switch the given alias in the path with the mapped alias.
                    // aliasPart.Alias -> AliasPartIndex.Alias
                    _usedAliases.Add(alias);
                    return Dialect.QuoteForTableName(alias) + "." + Dialect.QuoteForColumnName(values[1]);
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