using System.Collections.Generic;
using System.Threading.Tasks;
using OrchardCore.ContentManagement.Routing;

namespace OrchardCore.Contents.Routing
{
    public class ContentRoutingValidationCoordinator : IContentRoutingValidationCoordinator
    {
        private readonly IEnumerable<IContentRouteValidationProvider> _contentRouteValidationProviders;

        public ContentRoutingValidationCoordinator(IEnumerable<IContentRouteValidationProvider> contentRouteValidationProviders)
        {
            _contentRouteValidationProviders = contentRouteValidationProviders;
        }

        public async Task<bool> IsPathUniqueAsync(string path, string contentItemId)
        {
            foreach (var contentRouteProvider in _contentRouteValidationProviders)
            {
                var result = await contentRouteProvider.IsPathUniqueAsync(path, contentItemId);
                if (result == false)
                {
                    return result;
                }
            }

            return true;
        }
    }
}
