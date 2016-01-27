using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public abstract class ContentPartDisplayDriver<TPart> : DisplayDriver<TPart>, IContentDisplayDriver where TPart : ContentPart, new()
    {
        Task<IDisplayResult> IDisplayDriver<ContentItem>.BuildDisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            var part = model.As<TPart>();
            if(part != null)
            {
                return DisplayAsync(part);
            }

            return Task.FromResult<IDisplayResult>(null);
        }

        Task<IDisplayResult> IDisplayDriver<ContentItem>.BuildEditorAsync(ContentItem model, BuildEditorContext context)
        {
            var part = model.As<TPart>();
            if (part != null)
            {
                return EditAsync(part);
            }

            return Task.FromResult<IDisplayResult>(null);
        }

        Task<IDisplayResult> IDisplayDriver<ContentItem>.UpdateEditorAsync(ContentItem model, UpdateEditorContext context)
        {
            var part = model.As<TPart>();
            if (part != null)
            {
                var result = UpdateAsync(part, context.Updater);
                if (context.Updater.ModelState.IsValid)
                {
                    model.Weld(typeof(TPart).Name, part);
                }
                return result;
            }

            return Task.FromResult<IDisplayResult>(null);
        }
    }
}
