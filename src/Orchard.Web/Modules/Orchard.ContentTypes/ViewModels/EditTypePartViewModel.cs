using System.Collections.Generic;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Newtonsoft.Json.Linq;

namespace Orchard.ContentTypes.ViewModels
{
    public class EditTypePartViewModel
    {
        public EditTypePartViewModel()
        {
            Settings = new JObject();
        }

        public EditTypePartViewModel(int index, ContentTypePartDefinition part)
        {
            Index = index;
            PartDefinition = new EditPartViewModel(part.PartDefinition);
            Settings = part.Settings;
            PartSettings = part.PartDefinition.Settings;
            _Definition = part;
        }

        public int Index { get; set; }
        public string Prefix { get { return "Parts[" + Index + "]"; } }
        public EditPartViewModel PartDefinition { get; set; }
        public JObject PartSettings { get; set; }
        public JObject Settings { get; set; }
        public EditTypeViewModel Type { get; set; }
        public IEnumerable<TemplateViewModel> Templates { get; set; }
        public ContentTypePartDefinition _Definition { get; private set; }

        public string Description
        {
            get { return PartSettings["ContentPartSettings.Description"].Value<string>() ?? ""; }
        }
    }
}