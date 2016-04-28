using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public abstract class ContentFieldDisplayDriver<TField> : DisplayDriverBase, IContentFieldDisplayDriver where TField : ContentField, new()
    {
        Task<IDisplayResult> IContentFieldDisplayDriver.BuildDisplayAsync(string fieldName, ContentPart contentPart, BuildDisplayContext context)
        {
            var field = contentPart.Get<TField>(fieldName);
            if(field != null)
            {
                return DisplayAsync(field, contentPart);
            }

            return Task.FromResult(default(IDisplayResult));
        }

        Task<IDisplayResult> IContentFieldDisplayDriver.BuildEditorAsync(string fieldName, ContentPart contentPart, BuildEditorContext context)
        {
            var field = contentPart.Get<TField>(fieldName);
            if (field != null)
            {
                return EditAsync(field, contentPart);
            }

            return Task.FromResult(default(IDisplayResult));
        }

        Task<IDisplayResult> IContentFieldDisplayDriver.UpdateEditorAsync(string fieldName, ContentPart contentPart, UpdateEditorContext context)
        {
            var field = contentPart.Get<TField>(fieldName);
            if (field != null)
            {
                var result = UpdateAsync(field, contentPart, context.Updater);
                if (context.Updater.ModelState.IsValid)
                {
                    contentPart.Weld(fieldName, field);
                }

                return result;
            }

            return Task.FromResult(default(IDisplayResult));
        }

        public virtual Task<IDisplayResult> DisplayAsync(TField field, ContentPart part)
        {
            return Task.FromResult(Display(field, part));
        }

        public virtual Task<IDisplayResult> EditAsync(TField field, ContentPart part)
        {
            return Task.FromResult(Edit(field, part));
        }

        public virtual Task<IDisplayResult> UpdateAsync(TField field, ContentPart part, IUpdateModel updater)
        {
            return Task.FromResult(Update(field, part, updater));
        }

        public virtual IDisplayResult Display(TField field, ContentPart part)
        {
            return null;
        }

        public virtual IDisplayResult Edit(TField field, ContentPart part)
        {
            return null;
        }

        public virtual IDisplayResult Update(TField field, ContentPart part, IUpdateModel updater)
        {
            return null;
        }


    }
}
