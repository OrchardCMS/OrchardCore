using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Display.Handlers;
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

        public Task UpdateEditorAsync(UpdateEditorContext context)
        {
            _drivers.Invoke(driver => {
                var result = driver.UpdateEditor(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);

            return Task.CompletedTask;
        }
    }
}
