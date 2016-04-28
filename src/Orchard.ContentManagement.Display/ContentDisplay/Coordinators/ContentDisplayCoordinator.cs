using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.Coordinators
{
    /// <summary>
    /// Coordinates all implementations of <see cref="IContentDisplayDriver"/> to render a <see cref="ContentItem"/>.
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
