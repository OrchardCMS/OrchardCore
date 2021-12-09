using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class YoutubeFieldDisplayViewModel
    {
        public string EmbeddedAddress => Field.EmbeddedAddress;
        public string RawAddress => Field.RawAddress;
        public YoutubeField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
