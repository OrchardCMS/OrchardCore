using System.Collections.Generic;
using Orchard.Templates.Models;

namespace Orchard.Templates.ViewModels
{
    public class TemplateIndexViewModel
    {
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
