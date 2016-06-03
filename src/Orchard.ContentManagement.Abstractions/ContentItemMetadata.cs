using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;

namespace Orchard.ContentManagement
{
    public class ContentItemMetadata
    {
        public ContentItemMetadata()
        {
            Identity = new ContentIdentity();
        }

        public string DisplayText { get; set; }
        public ContentIdentity Identity { get; set; }
        public RouteValueDictionary DisplayRouteValues { get; set; }
        public RouteValueDictionary EditorRouteValues { get; set; }
        public RouteValueDictionary CreateRouteValues { get; set; }
        public RouteValueDictionary RemoveRouteValues { get; set; }
        public RouteValueDictionary AdminRouteValues { get; set; }

        public readonly IList<GroupInfo> DisplayGroupInfo = new List<GroupInfo>();
        public readonly IList<GroupInfo> EditorGroupInfo = new List<GroupInfo>();

    }
}
