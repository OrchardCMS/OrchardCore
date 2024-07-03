using System;
using OrchardCore.ContentManagement;
using OrchardCore.Queries;

namespace OrchardCore.Search.Lucene
{
    [Obsolete("This class will be removed in future release. Instead use Query.")]
    public class LuceneQuery : Query
    {
        public LuceneQuery() : base(LuceneQuerySource.SourceName)
        {
        }

        [Obsolete("Use .As<LuceneQueryMetadata>() instead to get this property value.")]
        public string Index { get; set; }

        [Obsolete("Use .As<LuceneQueryMetadata>() instead to get this property value.")]
        public string Template { get; set; }

        public override bool ResultsOfType<T>() => ReturnContentItems ? typeof(T) == typeof(ContentItem) : base.ResultsOfType<T>();
    }
}
