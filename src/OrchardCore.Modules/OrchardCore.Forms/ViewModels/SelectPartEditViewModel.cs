using OrchardCore.Forms.Models;

namespace OrchardCore.Forms.ViewModels
{
    public class SelectPartEditViewModel
    {
        public string Options { get; set; }
        public string DefaultValue { get; set; }
        public SelectEditorOption Editor { get; set; }
    }
}
