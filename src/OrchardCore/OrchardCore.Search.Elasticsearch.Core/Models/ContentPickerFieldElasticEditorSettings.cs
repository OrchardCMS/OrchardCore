using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.Elasticsearch.Core.Models
{
    public class ContentPickerFieldElasticEditorSettings
    {
        public string Index { get; set; }

        [BindNever]
        public string[] Indices { get; set; }
    }
}
