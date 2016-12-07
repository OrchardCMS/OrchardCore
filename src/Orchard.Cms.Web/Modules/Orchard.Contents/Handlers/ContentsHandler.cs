using Microsoft.AspNetCore.Routing;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;

namespace Orchard.Contents.Handlers
{
    public class ContentsHandler : ContentHandlerBase
    {
        public override void GetContentItemAspect(ContentItemAspectContext context)
        {
            context.For<ContentItemMetadata>(metadata =>
            {
                if (metadata.CreateRouteValues == null)
                {
                    metadata.CreateRouteValues = new RouteValueDictionary {
                        {"Area", "Orchard.Contents"},
                        {"Controller", "Admin"},
                        {"Action", "Create"},
                        {"Id", context.ContentItem.ContentType}
                    };
                }

                if (metadata.EditorRouteValues == null)
                {
                    metadata.EditorRouteValues = new RouteValueDictionary {
                        {"Area", "Orchard.Contents"},
                        {"Controller", "Admin"},
                        {"Action", "Edit"},
                        {"ContentItemId", context.ContentItem.ContentItemId}
                    };
                }

                if (metadata.AdminRouteValues == null)
                {
                    metadata.AdminRouteValues = new RouteValueDictionary {
                        {"Area", "Orchard.Contents"},
                        {"Controller", "Admin"},
                        {"Action", "Edit"},
                        {"ContentItemId", context.ContentItem.ContentItemId}
                    };
                }

                if (metadata.DisplayRouteValues == null)
                {
                    metadata.DisplayRouteValues = new RouteValueDictionary {
                        {"Area", "Orchard.Contents"},
                        {"Controller", "Item"},
                        {"Action", "Display"},
                        {"ContentItemId", context.ContentItem.ContentItemId}
                    };
                }

                if (metadata.RemoveRouteValues == null)
                {
                    metadata.RemoveRouteValues = new RouteValueDictionary {
                        {"Area", "Orchard.Contents"},
                        {"Controller", "Admin"},
                        {"Action", "Remove"},
                        {"ContentItemId", context.ContentItem.ContentItemId}
                    };
                }
            });
        }
    }
}