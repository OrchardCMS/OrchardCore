using System;
using System.Collections.Generic;
using System.Reflection;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Environment.Shell;
using YesSql;

namespace OrchardCore.ContentManagement.GraphQL.Queries.Predicates
{
    public class PredicateQuery : IPredicateQuery
    {
        private static Dictionary<string, string> _contentItemIndexProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _usedAliases = new HashSet<string>();
        private readonly Dictionary<string, string> _aliases = new Dictionary<string, string>();
        private readonly string _tablePrefix;

        public PredicateQuery(ISqlDialect dialect, ShellSettings shellSettings)
        {
            Dialect = dialect;

            var tablePrefix = shellSettings["TablePrefix"];
            _tablePrefix = string.IsNullOrEmpty(tablePrefix) ? String.Empty : $"{tablePrefix}_";
        }

        public ISqlDialect Dialect { get; set; }

        public IDictionary<string, object> Parameters { get; } = new Dictionary<string, object>();

        static PredicateQuery()
        {
            foreach (var property in typeof(ContentItemIndex).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase))
            {
                _contentItemIndexProperties[property.Name] = property.Name;
            }
        }

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
            if (values.Length == 1)
            {
                if (_aliases.TryGetValue(string.Empty, out alias))
                {
                    // Return the default alias
                    // contentItemId -> ContentItemIndex.ContentItemId

                    if (_contentItemIndexProperties.TryGetValue(values[0], out var columnName))
                    {
                        _usedAliases.Add(alias);
                        return Dialect.QuoteForTableName($"{_tablePrefix}{alias}") + "." + Dialect.QuoteForColumnName(columnName);
                    }
                }
            }
            else
            {
                if (_aliases.TryGetValue(values[0], out alias))
                {
                    // Switch the given alias in the path with the mapped alias.
                    // aliasPart.Alias -> AliasPartIndex.Alias
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
