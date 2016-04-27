using System;
using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public abstract class ContentPartDisplayDriver<TPart> : DisplayDriver<TPart, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>, IContentDisplayDriver where TPart : ContentPart, new()
    {
        Task<IDisplayResult> IDisplayDriver<ContentItem, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>.BuildDisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            var part = model.As<TPart>();
            if(part != null)
            {
                return DisplayAsync(part, context.Updater);
            }

            return Task.FromResult<IDisplayResult>(null);
        }

        Task<IDisplayResult> IDisplayDriver<ContentItem, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>.BuildEditorAsync(ContentItem model, BuildEditorContext context)
        {
            var part = model.As<TPart>();
            if (part != null)
            {
                return EditAsync(part, context.Updater);
            }

            return Task.FromResult<IDisplayResult>(null);
        }

        Task<IDisplayResult> IDisplayDriver<ContentItem, BuildDisplayContext, BuildEditorContext, UpdateEditorContext>.UpdateEditorAsync(ContentItem model, UpdateEditorContext context)
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
