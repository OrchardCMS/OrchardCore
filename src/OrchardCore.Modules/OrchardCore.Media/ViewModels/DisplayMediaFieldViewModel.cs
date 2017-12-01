using OrchardCore.Media.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.Media.ViewModels
{
    public class DisplayMediaFieldViewModel
    {
        private string[] _publicUrls;

        public string[] PublicUrls
        {
            get { return _publicUrls ?? new string[0]; }
            set { _publicUrls = value; }
        }

        public MediaField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
