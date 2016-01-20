using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.Coordinators
{
    /// <summary>
    /// This component coordinates how content element display components are taking part in the rendering when some content needs to be rendered.
    /// It will dispatch BuildDisplay/BuildEditor to all <see cref="IContentElementDisplay"/> implementations.
    /// </summary>
    public class ContentDisplayCoordinator : IDisplayHandler
    {
        private readonly IEnumerable<IContentElementDisplay> _drivers;

        public ContentDisplayCoordinator(
            IEnumerable<IContentElementDisplay> drivers,
            ILogger<ContentDisplayCoordinator> logger)
        {
            _drivers = drivers;

            Logger = logger;
        }

        public ILogger Logger { get; set; }
        
        public Task BuildDisplayAsync(BuildDisplayContext context)
        {
            return _drivers.InvokeAsync(async driver => {
                var result = await driver.BuildDisplayAsync(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildEditorAsync(BuildEditorContext context)
        {
            return _drivers.InvokeAsync(async driver => {
                var result = await driver.BuildEditorAsync(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateEditorAsync(UpdateEditorContext context, IModelUpdater modelUpdater)
        {
            return _drivers.InvokeAsync(async driver => {
                var result = await driver.UpdateEditorAsync(context, modelUpdater);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }
}
