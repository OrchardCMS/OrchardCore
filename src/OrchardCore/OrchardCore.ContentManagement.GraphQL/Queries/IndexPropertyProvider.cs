using System;
using System.Collections.Generic;
using System.Reflection;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public class IndexPropertyProvider<T> : IIndexPropertyProvider where T : MapIndex
    {
        private static Dictionary<string, string> _indexProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static string _indexName;

        static IndexPropertyProvider()
        {
            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase))
            {
                _indexProperties[property.Name] = property.Name;
            }

            _indexName = typeof(T).Name;
        }

        public string IndexName => _indexName;

        public bool TryGetValue(string propertyName, out string indexPropertyName)
        {
            return _indexProperties.TryGetValue(propertyName, out indexPropertyName);
        }
    }
}
