using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IModelUpdaterAccessor _modelUpdaterAccessor;

        public ContentDisplayCoordinator(
            IEnumerable<IContentElementDisplay> drivers,
            IModelUpdaterAccessor modelUpdaterAccessor,
            ILogger<ContentDisplayCoordinator> logger)
        {
            _drivers = drivers;
            _modelUpdaterAccessor = modelUpdaterAccessor;

            Logger = logger;
        }

        public ILogger Logger { get; set; }
        
        public Task BuildDisplayAsync(BuildDisplayContext context)
        {
            _drivers.Invoke(driver => {
                var result = driver.BuildDisplay(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return Task.CompletedTask;
        }

        public Task BuildEditorAsync(BuildEditorContext context)
        {
            _drivers.Invoke(driver => {
                var result = driver.BuildEditor(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return Task.CompletedTask;
        }

        public async Task UpdateEditorAsync(UpdateEditorContext context)
        {
            await _drivers.InvokeAsync(
                driver => driver.UpdateEditorAsync(context, _modelUpdaterAccessor.ModelUpdater),
                Logger
                );
        }
    }
}
