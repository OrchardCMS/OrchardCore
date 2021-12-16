using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.Menu.Models;

namespace OrchardCore.Menu.ViewModels
{
    public class MenuPartEditViewModel
    {
        public string Hierarchy { get; set; }

        [BindNever]
        public MenuPart MenuPart { get; set; }

        [BindNever]
        public IEnumerable<ContentTypeDefinition> MenuItemContentTypes { get; set; }
    }
}
