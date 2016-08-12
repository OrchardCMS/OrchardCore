using System.Threading.Tasks;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    /// <summary>
    /// Any concrete implementation of this class can provide shapes for any content item which has a specific Part.
    /// </summary>
    /// <typeparam name="TPart"></typeparam>
    public abstract class ContentPartDisplayDriver<TPart> : DisplayDriverBase, IContentPartDisplayDriver where TPart : ContentPart, new()
    {
        Task<IDisplayResult> IContentPartDisplayDriver.BuildDisplayAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
        {
            if (!string.Equals(typeof(TPart).Name, typePartDefinition.PartDefinition.Name) &&
                typeof(TPart) != typeof(ContentPart))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var part = contentPart.ContentItem.Get<TPart>(typePartDefinition.Name);
            var buildDisplayContext = new BuildPartDisplayContext(typePartDefinition, context);

            return DisplayAsync(part, buildDisplayContext);
        }

        Task<IDisplayResult> IContentPartDisplayDriver.BuildEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
        {
            if (!string.Equals(typeof(TPart).Name, typePartDefinition.PartDefinition.Name) &&
                typeof(TPart) != typeof(ContentPart))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var part = contentPart.ContentItem.GetOrCreate<TPart>(typePartDefinition.Name);
            var buildEditorContext = new BuildPartEditorContext(typePartDefinition, context);

            return EditAsync(part, buildEditorContext);
        }

        Task<IDisplayResult> IContentPartDisplayDriver.UpdateEditorAsync(ContentPart contentPart, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
        {
            if (!string.Equals(typeof(TPart).Name, typePartDefinition.PartDefinition.Name) &&
                typeof(TPart) != typeof(ContentPart))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var part = contentPart.ContentItem.GetOrCreate<TPart>(typePartDefinition.Name);
            var updateEditorContext = new UpdatePartEditorContext(typePartDefinition, context);

            return UpdateAsync(part, context.Updater, updateEditorContext);
        }

        public virtual Task<IDisplayResult> DisplayAsync(TPart part, BuildPartDisplayContext context)
        {
            return Task.FromResult(Display(part, context));
        }

        public virtual IDisplayResult Display(TPart part, BuildPartDisplayContext context)
        {
            return Display(part);
        }

        public virtual IDisplayResult Display(TPart part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> EditAsync(TPart part, BuildPartEditorContext context)
        {
            return Task.FromResult(Edit(part, context));
        }

        public virtual IDisplayResult Edit(TPart part, BuildPartEditorContext context)
        {
            return Edit(part);
        }

        public virtual IDisplayResult Edit(TPart part)
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, IUpdateModel updater, UpdatePartEditorContext context)
        {
            return UpdateAsync(part, context);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, BuildPartEditorContext context)
        {
            return UpdateAsync(part, context.Updater);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TPart part, IUpdateModel updater)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

    }
}
