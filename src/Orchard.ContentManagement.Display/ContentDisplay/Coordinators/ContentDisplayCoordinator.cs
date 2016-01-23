using Microsoft.Extensions.Logging;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Orchard.ContentManagement.Display.Coordinators
{
    /// <summary>
    /// This component coordinates how content element display components are taking part in the rendering when some content needs to be rendered.
    /// It will dispatch BuildDisplay/BuildEditor to all <see cref="IContentDisplay"/> implementations.
    /// </summary>
    public class ContentDisplayCoordinator : IDisplayHandler<IContent>, IContentDisplayHandler
    {
        private readonly IEnumerable<IContentDisplay> _contentDisplays;

        public ContentDisplayCoordinator(
            IEnumerable<IContentDisplay> contentDisplays,
            ILogger<ContentDisplayCoordinator> logger)
        {
            _contentDisplays = contentDisplays;

            Logger = logger;
        }

        public ILogger Logger { get; set; }
        
        public Task BuildDisplayAsync(BuildDisplayContext<IContent> context)
        {
            return _contentDisplays.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildDisplayAsync(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task BuildEditorAsync(BuildEditorContext<IContent> context)
        {
            return _contentDisplays.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.BuildEditorAsync(context);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }

        public Task UpdateEditorAsync(UpdateEditorContext<IContent> context, IUpdateModel modelUpdater)
        {
            return _contentDisplays.InvokeAsync(async contentDisplay => {
                var result = await contentDisplay.UpdateEditorAsync(context, modelUpdater);
                if (result != null)
                    result.Apply(context);
            }, Logger);
        }
    }
}
