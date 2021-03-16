using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;

namespace OrchardCore.ContentTypes.ViewModels
{
    public class EditTypeViewModel
    {
        public EditTypeViewModel()
        {
            Settings = new JObject();
        }

        public EditTypeViewModel(ContentTypeDefinition contentTypeDefinition)
        {
            Name = contentTypeDefinition.Name;
            DisplayName = contentTypeDefinition.DisplayName;
            Settings = contentTypeDefinition.Settings;
            TypeDefinition = contentTypeDefinition;
        }

        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string[] OrderedFieldNames { get; set; }
        public string[] OrderedPartNames { get; set; }

        [BindNever]
        public JObject Settings { get; set; }

        [BindNever]
        public ContentTypeDefinition TypeDefinition { get; set; }

        [BindNever]
        public dynamic Editor { get; set; }
    }
}
