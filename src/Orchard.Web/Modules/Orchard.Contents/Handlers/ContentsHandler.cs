using Orchard.ContentManagement.Handlers;
using Microsoft.AspNetCore.Routing;

namespace Orchard.Contents.Handlers
{
    public class ContentsHandler : ContentHandlerBase
    {
        public override void GetContentItemMetadata(ContentItemMetadataContext context)
        {

            if (context.Metadata.CreateRouteValues == null)
            {
                context.Metadata.CreateRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Create"},
                    {"Id", context.ContentItem.ContentType}
                };

            }

            if (context.Metadata.EditorRouteValues == null)
            {
                context.Metadata.EditorRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Edit"},
                    {"Id", context.ContentItem.ContentItemId}
                };
            }

            if (context.Metadata.AdminRouteValues == null)
            {
                context.Metadata.AdminRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Edit"},
                    {"Id", context.ContentItem.ContentItemId}
                };
            }

            if (context.Metadata.DisplayRouteValues == null)
            {
                context.Metadata.DisplayRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Contents"},
                    {"Controller", "Item"},
                    {"Action", "Display"},
                    {"Id", context.ContentItem.ContentItemId}
                };
            }

            if (context.Metadata.RemoveRouteValues == null)
            {
                context.Metadata.RemoveRouteValues = new RouteValueDictionary {
                    {"Area", "Orchard.Contents"},
                    {"Controller", "Admin"},
                    {"Action", "Remove"},
                    {"Id", context.ContentItem.ContentItemId}
                };
            }
        }
    }
}