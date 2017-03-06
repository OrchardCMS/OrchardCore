using System.Threading.Tasks;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    /// <summary>
    /// Any concrete implementation of this class can provide shapes for any part of a content item.
    /// </summary>
    public abstract class ContentPartDisplayDriver : DisplayDriverBase, IContentPartDisplayDriver
    {
        Task<IDisplayResult> IContentPartDisplayDriver.BuildDisplayAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
        {
            var buildDisplayContext = new BuildPartDisplayContext(typePartDefinition, context);

            return DisplayAsync(contentPart, buildDisplayContext);
        }

        Task<IDisplayResult> IContentPartDisplayDriver.BuildEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
        {
            var buildEditorContext = new BuildPartEditorContext(typePartDefinition, context);

            return EditAsync(contentPart, buildEditorContext);
        }

        Task<IDisplayResult> IContentPartDisplayDriver.UpdateEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
        {
            var updateEditorContext = new UpdatePartEditorContext(typePartDefinition, context);

            return UpdateAsync(contentPart, context.Updater, updateEditorContext);
        }

        public virtual Task<IDisplayResult> DisplayAsync(ContentPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult(Display(part, context));
        }

        public virtual IDisplayResult Display(ContentPart part, BuildPartDisplayContext context)
        {
            return Display(part);
        }

        public virtual IDisplayResult Display(ContentPart part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> EditAsync(ContentPart part, BuildPartEditorContext context)
        {
            return Task.FromResult(Edit(part, context));
        }

        public virtual IDisplayResult Edit(ContentPart part, BuildPartEditorContext context)
        {
            return Edit(part);
        }

        public virtual IDisplayResult Edit(ContentPart part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(ContentPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            return UpdateAsync(part, context);
        }

        public virtual Task<IDisplayResult> UpdateAsync(ContentPart part, BuildPartEditorContext context)
        {
            return UpdateAsync(part, context.Updater);
        }

        public virtual Task<IDisplayResult> UpdateAsync(ContentPart part, IUpdateModel updater)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

    }

}
