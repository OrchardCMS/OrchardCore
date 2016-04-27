using System.Collections.Generic;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;
using Orchard.ContentFields.Settings;

namespace Orchard.ContentFields.Events
{
    public class TextFieldEvents : ContentDefinitionEditorEventsBase
    {
        public override IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition)
        {
            var model = definition.Settings.ToObject<TextFieldSettings>();
            yield return DefinitionTemplate(model);
        } 
    }
}