using System;
using System.Collections.Generic;
using System.Text;
using YesSql.Indexes;

namespace OrchardCore.ContentManagement.GraphQL.Queries
{
    public interface IIndexPropertyProvider
    {
        public string IndexName { get; }
        public bool TryGetValue(string propertyName, out string indexPropertyName);
    }
}
