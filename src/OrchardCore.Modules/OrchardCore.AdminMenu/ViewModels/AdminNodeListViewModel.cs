using System.Collections.Generic;

namespace OrchardCore.AdminMenu.ViewModels
{
    public class AdminNodeListViewModel
    {
        public Models.AdminMenu AdminMenu { get; set; }
        public IDictionary<string, dynamic> Thumbnails { get; set; }
    }
}
