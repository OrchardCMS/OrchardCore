using System.Threading.Tasks;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public abstract class ContentFieldDisplayDriver<TField> : DisplayDriverBase, IContentFieldDisplayDriver where TField : ContentField, new()
    {
        Task<IDisplayResult> IContentFieldDisplayDriver.BuildDisplayAsync(string fieldName, ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
        {
            if(!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
               !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var field = contentPart.Get<TField>(fieldName);
            if (field != null)
            {
                Prefix = typePartDefinition.Name + "." + partFieldDefinition.Name;
                return DisplayAsync(field, contentPart, partFieldDefinition);
            }

            return Task.FromResult(default(IDisplayResult));
        }

        Task<IDisplayResult> IContentFieldDisplayDriver.BuildEditorAsync(string fieldName, ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
        {
            if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
                !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var field = contentPart.Get<TField>(fieldName);
            if (field != null)
            {
                Prefix = typePartDefinition.Name + "." + partFieldDefinition.Name;
                return EditAsync(field, contentPart, partFieldDefinition);
            }

            return Task.FromResult(default(IDisplayResult));
        }

        async Task<IDisplayResult> IContentFieldDisplayDriver.UpdateEditorAsync(string fieldName, ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
        {
            if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
                !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return null;
            }

            var field = contentPart.Get<TField>(fieldName);

            if (field != null)
            {
                Prefix = typePartDefinition.Name + "." + partFieldDefinition.Name;
                var result = await UpdateAsync(field, contentPart, partFieldDefinition, context.Updater);

                if (result == null)
                {
                    return null;
                }

                if (context.Updater.ModelState.IsValid)
                {
                    contentPart.Weld(fieldName, field);
                }

                return result;
            }

            return null;
        }

        public virtual Task<IDisplayResult> DisplayAsync(TField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            return Task.FromResult(Display(field, part, partFieldDefinition));
        }

        public virtual Task<IDisplayResult> EditAsync(TField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            return Task.FromResult(Edit(field, part, partFieldDefinition));
        }

        public virtual Task<IDisplayResult> UpdateAsync(TField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition, IUpdateModel updater)
        {
            return Task.FromResult(Update(field, part, partFieldDefinition, updater));
        }

        public virtual IDisplayResult Display(TField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            return null;
        }

        public virtual IDisplayResult Edit(TField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition)
        {
            return null;
        }

        public virtual IDisplayResult Update(TField field, ContentPart part, ContentPartFieldDefinition partFieldDefinition, IUpdateModel updater)
        {
            return null;
        }

    }
}
