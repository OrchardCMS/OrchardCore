using Microsoft.AspNetCore.Mvc.ModelBinding;
using OrchardCore.ContainerRoute.Models;
using OrchardCore.ContentManagement;

namespace OrchardCore.ContainerRoute.ViewModels
{
    public class RouteHandlerPartViewModel
    {
        public string Path { get; set; }
        public bool IsRelative { get; set; }
        public bool IsRoutable { get; set; }
        public bool UpdatePath { get; set; }

        [BindNever]
        public ContentItem ContentItem { get; set; }

        [BindNever]
        public RouteHandlerPart RouteHandlerPart { get; set; }

        [BindNever]
        public RouteHandlerPartSettings Settings { get; set; }
    }
}
