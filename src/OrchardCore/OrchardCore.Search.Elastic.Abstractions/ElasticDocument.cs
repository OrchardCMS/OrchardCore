using System;
using System.Collections.Generic;
using System.Text;

namespace OrchardCore.Search.Elastic
{ 
    public class ElasticDocument
    {
        public ElasticDocument(string contentItemId)
        {
            ContentItemId = contentItemId;
            Set("ContentItemId",contentItemId);
            Id = contentItemId;
        }

        public Dictionary<string, object> Fields  = new Dictionary<string, object>();
        public void Set(string name, object value)
        {
            Fields.Add(name, value);
        }
        public object Get(string name)
        {
           return Fields.GetValueOrDefault(name);
        }
        public string ContentItemId { get; }
        public string Id { get; }
    }
}
