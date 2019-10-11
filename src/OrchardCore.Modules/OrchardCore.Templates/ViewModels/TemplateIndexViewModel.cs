using System.Collections.Generic;
using OrchardCore.Templates.Models;

namespace OrchardCore.Templates.ViewModels
{
    public class TemplateIndexViewModel
    {
        public bool AdminTemplates { get; set; }
        public IList<TemplateEntry> Templates { get; set; }
        public dynamic Pager { get; set; }
    }

    public class TemplateEntry
    {
        public string Name { get; set; }
        public Template Template { get; set; }
        public bool IsChecked { get; set; }
    }
}
