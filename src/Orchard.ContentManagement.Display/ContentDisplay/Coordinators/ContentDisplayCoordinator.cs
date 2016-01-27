using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using System.Collections.Generic;
using Orchard.DisplayManagement.Handlers;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.Coordinators
{
    /// <summary>
    /// Provides a concrete implementation of a display coordinator managing <see cref="IContentDisplayDriver"/>
    /// implementations.
    /// </summary>
    public class ContentDisplayCoordinator : IContentDisplayHandler
    {
        private readonly IEnumerable<IContentDisplayDriver> _displayDrivers;

        public ContentDisplayCoordinator(
            IEnumerable<IContentDisplayDriver> displayDrivers,
            ILogger<ContentDisplayCoordinator> logger)
        {
            _displayDrivers = displayDrivers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }

        public Task BuildDisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            return _displayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildEditorAsync(ContentItem model, BuildEditorContext context)
        {
            return _displayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateEditorAsync(ContentItem model, UpdateEditorContext context)
        {
            return _displayDrivers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }
}
