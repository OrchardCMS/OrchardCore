using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.ContentTypes.ViewModels
{
    public class EditPartViewModel
    {
        public EditPartViewModel()
        {
            Settings = new JObject();
        }

        public EditPartViewModel(ContentPartDefinition contentPartDefinition)
        {
            Name = contentPartDefinition.Name;
            Settings = contentPartDefinition.Settings;
            PartDefinition = contentPartDefinition;
        }

        public string Name { get; set; }

        private string _displayName;
        [Required]
        public string DisplayName
        {
            get { return !string.IsNullOrWhiteSpace(_displayName) ? _displayName : Name.TrimEnd("Part").CamelFriendly(); }
            set { _displayName = value; }
        }

        public string Description
        {
            get { return Settings.Value<string>("Description"); }
            set { Settings["Description"] = value; }
        }

        [BindNever]
        public JObject Settings { get; set; }

        [BindNever]
        public ContentPartDefinition PartDefinition { get; private set; }

        [BindNever]
        public dynamic Editor { get; set; }
    }
}