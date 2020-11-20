using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentFields.ViewModels
{
    public class DisplayUserPickerFieldUserNamesViewModel
    {
        public string[] UserIds => Field.UserIds;
        public string[] UserNames => Field.GetUserNames();
        public UserPickerField Field { get; set; }
        public ContentPart Part { get; set; }
        public ContentPartFieldDefinition PartFieldDefinition { get; set; }
    }
}
