using System;
using System.Threading.Tasks;
using OrchardCore.ContainerRoute.Routing;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Liquid;

namespace OrchardCore.ContainerRoute.Handlers
{
    public class DefaultRouteHandler : ContentHandlerBase
    {
        private readonly ISlugService _slugService;

        public DefaultRouteHandler(ISlugService slugService)
        {
            _slugService = slugService;
        }

        public override Task GetContentItemAspectAsync(ContentItemAspectContext context)
        {
            return context.ForAsync<RouteHandlerAspect>(aspect =>
            {
                // Only use default aspect if no other handler has set the aspect.
                if (String.IsNullOrEmpty(aspect.Path))
                {
                    // By default contained route is content item id plus, if present, display text.
                    var path = context.ContentItem.ContentItemId;
                    if (!String.IsNullOrEmpty(context.ContentItem.DisplayText))
                    {
                        path = path + "-" + context.ContentItem.DisplayText;
                    }

                    aspect.Path = _slugService.Slugify(path);
                }

                return Task.CompletedTask;
            });
        }
    }
}
