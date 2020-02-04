using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContainerRoute.ViewModels
{
    public class ContainerRoutePartViewModel
    {
        public string Path { get; set; }
        public bool SetHomepage { get; set; }
        public bool UpdatePath { get; set; }
        public bool IsHomepage { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public ContainerRoutePart ContainerRoutePart { get; set; }

        [BindNever]
        public ContainerRoutePartSettings Settings { get; set; }
    }
}
