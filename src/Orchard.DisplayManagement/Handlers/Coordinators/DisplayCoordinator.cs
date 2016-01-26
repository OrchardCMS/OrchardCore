using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers.Coordinators
{
    /// <summary>
    /// This component coordinates <see cref="IDisplayDriver"/> instances by applying
    /// their <see cref="Orchard.DisplayManagement.Views.IDisplayResult"/> to be applied on the result shape.
    /// </summary>
    public abstract class DisplayCoordinator<TDisplayDriver> where TDisplayDriver : IDisplayDriver
    {
        private readonly IEnumerable<TDisplayDriver> _displayHandlers;

        public DisplayCoordinator(
            IEnumerable<TDisplayDriver> displayHandlers,
            ILogger logger)
        {
            _displayHandlers = displayHandlers;
            Logger = logger;
        }

        private ILogger Logger { get; set; }
        
        public Task BuildDisplayAsync(object model, BuildDisplayContext context)
        {
            return _displayHandlers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildEditorAsync(object model, BuildEditorContext context)
        {
            return _displayHandlers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateEditorAsync(object model, UpdateEditorContext context)
        {
            return _displayHandlers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }
}
