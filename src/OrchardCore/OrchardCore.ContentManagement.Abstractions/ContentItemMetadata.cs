using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.ContentManagement
{
    public class ContentItemMetadata
    {
        public RouteValueDictionary DisplayRouteValues { get; set; }
        public RouteValueDictionary EditorRouteValues { get; set; }
        public RouteValueDictionary CreateRouteValues { get; set; }
        public RouteValueDictionary RemoveRouteValues { get; set; }
        public RouteValueDictionary AdminRouteValues { get; set; }

        public readonly IList<GroupInfo> DisplayGroupInfo = new List<GroupInfo>();
        public readonly IList<GroupInfo> EditorGroupInfo = new List<GroupInfo>();
    }
}
