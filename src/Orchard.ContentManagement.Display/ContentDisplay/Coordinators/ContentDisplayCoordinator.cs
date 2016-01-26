using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.Handlers.Coordinators;
using System.Collections.Generic;

namespace Orchard.ContentManagement.Display.Coordinators
{
    /// <summary>
    /// This component coordinates how content element display components are taking part in the rendering when some content needs to be rendered.
    /// It will dispatch BuildDisplay/BuildEditor to all <see cref="IContentDisplay"/> implementations.
    /// </summary>
    public class ContentDisplayCoordinator : DisplayCoordinator<IContentDisplayDriver>, IContentDisplayHandler
    {
        private readonly IEnumerable<IContentDisplayDriver> _contentDisplayHandlers;

        public ContentDisplayCoordinator(
            IEnumerable<IContentDisplayDriver> contentDisplayHandlers,
            ILogger<ContentDisplayCoordinator> logger)
            :base(contentDisplayHandlers, logger)
        {
            _contentDisplayHandlers = contentDisplayHandlers;
        }
    }
}
