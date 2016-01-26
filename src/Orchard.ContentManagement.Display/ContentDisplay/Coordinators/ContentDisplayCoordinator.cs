using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.Handlers.Coordinators;
using System.Collections.Generic;

namespace Orchard.ContentManagement.Display.Coordinators
{
    /// <summary>
    /// Provides a concrete implementation of a display coordinator managing <see cref="IContentDisplayDriver"/>
    /// implementations.
    /// </summary>
    public class ContentDisplayCoordinator : DisplayCoordinator<ContentItem, IContentDisplayDriver>, IContentDisplayHandler
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
