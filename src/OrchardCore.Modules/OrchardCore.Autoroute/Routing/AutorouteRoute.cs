using System.Threading.Tasks;
using Microsoft.AspNetCore.Routing;
using OrchardCore.ContentManagement;

namespace OrchardCore.Autoroute.Routing
{
    public class AutorouteRoute
    {
        private readonly IContentManager _contentManager;

        public AutorouteRoute(IContentManager contentManager)
        {
            _contentManager = contentManager;
        }

        public async Task<RouteValueDictionary> GetValuesAsync(string contentItemId)
        {
            if (string.IsNullOrEmpty(contentItemId))
            {
                return null;
            }

            var contentItem = await _contentManager.GetAsync(contentItemId);

            if (contentItem == null)
            {
                return null;
            }

            return (await _contentManager.PopulateAspectAsync<ContentItemMetadata>(contentItem))?.DisplayRouteValues;
        }
    }
}
