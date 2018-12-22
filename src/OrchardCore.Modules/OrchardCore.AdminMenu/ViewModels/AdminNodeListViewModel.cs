using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.AdminMenu.Models;

namespace OrchardCore.AdminMenu.ViewModels
{
    public class AdminNodeListViewModel
    {
        public AdminTree AdminTree { get; set; }
        public IDictionary<string, dynamic> Thumbnails { get; set; }
    }
}
