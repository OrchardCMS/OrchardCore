using System;
using OrchardCore.ContentManagement;
using OrchardCore.Queries;
using OrchardCore.Search.Elasticsearch.Core.Services;

namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    [Obsolete("This class will be removed in future release. Instead use Query.")]
    public class ElasticQuery : Query
    {
        public ElasticQuery() : base(ElasticQuerySource.SourceName) { }

        [Obsolete("Use .As<ElasticsearchQueryMetadata>() instead to get this property value.")]
        public string Index { get; set; }

        [Obsolete("Use .As<ElasticsearchQueryMetadata>() instead to get this property value.")]
        public string Template { get; set; }

        public override bool ResultsOfType<T>() => ReturnContentItems ? typeof(T) == typeof(ContentItem) : base.ResultsOfType<T>();
    }
}
