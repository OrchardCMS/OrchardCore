using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.Data;
using YesSql.Indexes;

namespace OrchardCore.ContentFields.Indexing
{
    public abstract class ContentFieldIndex : MapIndex
    {
        public string ContentType { get; set; }
        public string ContentPart { get; set; }
        public string ContentField { get; set; }
    }

    public abstract class ContentFieldIndexProvider : IndexProvider<ContentItem>, IScopedIndexProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly HashSet<string> _ignoredTypes = new HashSet<string>();

        public ContentFieldIndexProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }
}