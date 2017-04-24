using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Orchard.ContentTypes.ViewModels
{
    public class ListContentPartsViewModel
    {
        [BindNever]
        public IEnumerable<EditPartViewModel> Parts { get; set; }
    }
}