using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Taxonomies.Models;

namespace OrchardCore.Taxonomies.ViewModels
{
    public class TaxonomyPartViewModel 
    {
        public TaxonomyPart TaxonomyPart { get; set; }
        public BuildPartDisplayContext BuildPartDisplayContext { get; set; }
        public List<TermEntryViewModel> Terms { get; set; }
    }
}
