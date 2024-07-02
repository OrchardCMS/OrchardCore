using System;
using OrchardCore.ContentManagement;

namespace OrchardCore.Queries.Sql
{
    [Obsolete("This class will be removed in future release. Instead use Query.")]
    public class SqlQuery : Query
    {
        public SqlQuery() : base(SqlQuerySource.SourceName)
        {
        }

        [Obsolete("Use .As<SqlQueryMetadata>() instead to get this property value.")]
        public string Template { get; set; }

        [Obsolete("Use .As<SqlQueryMetadata>() instead to get this property value.")]
        public bool ReturnDocuments { get; set; }

        public override bool ResultsOfType<T>() => ReturnDocuments ? typeof(T) == typeof(ContentItem) : base.ResultsOfType<T>();
    }
}
