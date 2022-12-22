using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Search.Lucene.Settings
{
    public class ContentPickerFieldLuceneEditorSettings
    {
        public string Index { get; set; }

        [BindNever]
        public string[] Indices { get; set; }
    }
}
