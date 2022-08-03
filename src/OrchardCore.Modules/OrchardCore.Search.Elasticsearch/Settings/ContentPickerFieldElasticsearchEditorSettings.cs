using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.Elasticsearch.Settings
{
    public class ContentPickerFieldElasticsearchEditorSettings
    {
        public string Index { get; set; }

        [BindNever]
        public string[] Indices { get; set; }
    }
}
