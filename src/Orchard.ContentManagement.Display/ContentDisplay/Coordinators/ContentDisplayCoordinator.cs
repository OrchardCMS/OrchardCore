using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.Handlers;

namespace Orchard.ContentManagement.Display.Coordinators
{
    /// <summary>
    /// Coordinates all implementations of <see cref="IContentDisplayDriver"/> to render a <see cref="ContentItem"/>.
    /// Drivers return display result objects that have their own logic to alter the rendering of a content item.
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

        public async Task BuildDisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.BuildDisplayAsync(model, context);
                    if (result != null)
                    {
                        result.Apply(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, nameof(BuildDisplayAsync));
                }
            }
        }

        public async Task BuildEditorAsync(ContentItem model, BuildEditorContext context)
        {
            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.BuildEditorAsync(model, context);
                    if (result != null)
                    {
                        result.Apply(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, nameof(BuildEditorAsync));
                }
            }
        }

        public async Task UpdateEditorAsync(ContentItem model, UpdateEditorContext context)
        {
            foreach (var displayDriver in _displayDrivers)
            {
                try
                {
                    var result = await displayDriver.UpdateEditorAsync(model, context);
                    if (result != null)
                    {
                        result.Apply(context);
                    }
                }
                catch (Exception ex)
                {
                    InvokeExtensions.HandleException(ex, Logger, displayDriver.GetType().Name, nameof(UpdateEditorAsync));
                }
            }
        }
    }
}
