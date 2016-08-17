using Orchard.ContentManagement.Metadata.Models;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.ContentTypes.ViewModels
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

        [BindNever]
        public JObject Settings { get; set; }

        [BindNever]
        public ContentTypeDefinition TypeDefinition { get; private set; }
    }
}