using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.DisplayManagement.Handlers.Coordinators
{
    /// <summary>
    /// This component coordinates <see cref="IDisplayDriver"/> instances by applying
    /// their <see cref="Orchard.DisplayManagement.Views.IDisplayResult"/> to be applied on the result shape.
    /// </summary>
    public abstract class DisplayCoordinator<TModel, TDisplayDriver> where TDisplayDriver : IDisplayDriver
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
        
        public Task BuildDisplayAsync(TModel model, BuildDisplayContext context)
        {
            return _displayHandlers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildEditorAsync(TModel model, BuildEditorContext context)
        {
            return _displayHandlers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateEditorAsync(TModel model, UpdateEditorContext context)
        {
            return _displayHandlers.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(model, context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }
}
