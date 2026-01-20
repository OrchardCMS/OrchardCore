using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Rules.ViewModels
{
    public class SelectStringOperationViewModel
    {
        public string HtmlName { get; set; }
        public List<SelectListItem> Items { get; set; }
    }
}
