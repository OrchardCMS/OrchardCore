using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Lists.AdminNodes
{
    public class ListsAdminNodeViewModel
    {
        public string ContentType { get; set; }
        public bool AddContentTypeAsParent { get; set; }
        public string IconForParentLink { get; set; }
        public string IconForContentItems { get; set; }
        public List<SelectListItem> ContentTypes { get; set; }
    }
}
