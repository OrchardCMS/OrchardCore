using System;
using System.Threading.Tasks;
using Orchard.ContentManagement.Display.Models;
using Orchard.ContentManagement.Metadata.Models;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.ContentManagement.Display.ContentDisplay
{
    public abstract class ContentFieldDisplayDriver<TField> : DisplayDriverBase, IContentFieldDisplayDriver where TField : ContentField, new()
    {
        Task<IDisplayResult> IContentFieldDisplayDriver.BuildDisplayAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildDisplayContext context)
        {
            if(!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
               !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var field = contentPart.Get<TField>(partFieldDefinition.Name);
            if (field != null)
            {
                Prefix = typePartDefinition.Name + "." + partFieldDefinition.Name;

                if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
                {
                    Prefix = context.HtmlFieldPrefix + "." + Prefix;
                }

                var fieldDisplayContext = new BuildFieldDisplayContext(contentPart, typePartDefinition, partFieldDefinition, context);
                return DisplayAsync(field, fieldDisplayContext);
            }

            return Task.FromResult(default(IDisplayResult));
        }

        Task<IDisplayResult> IContentFieldDisplayDriver.BuildEditorAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, BuildEditorContext context)
        {
            if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
                !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return Task.FromResult(default(IDisplayResult));
            }

            var field = contentPart.GetOrCreate<TField>(partFieldDefinition.Name);

            Prefix = typePartDefinition.Name + "." + partFieldDefinition.Name;

            if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
            {
                Prefix = context.HtmlFieldPrefix + "." + Prefix;
            }

            var fieldEditorContext = new BuildFieldEditorContext(contentPart, typePartDefinition, partFieldDefinition, context);

            return EditAsync(field, fieldEditorContext);
        }

        async Task<IDisplayResult> IContentFieldDisplayDriver.UpdateEditorAsync(ContentPart contentPart, ContentPartFieldDefinition partFieldDefinition, ContentTypePartDefinition typePartDefinition, UpdateEditorContext context)
        {
            if (!string.Equals(typeof(TField).Name, partFieldDefinition.FieldDefinition.Name) &&
                !string.Equals(nameof(ContentField), partFieldDefinition.FieldDefinition.Name))
            {
                return null;
            }

            var field = contentPart.GetOrCreate<TField>(partFieldDefinition.Name);

            Prefix = typePartDefinition.Name + "." + partFieldDefinition.Name;

            if (!String.IsNullOrEmpty(context.HtmlFieldPrefix))
            {
                Prefix = context.HtmlFieldPrefix + "." + Prefix;
            }

            var updateFieldEditorContext = new UpdateFieldEditorContext(contentPart, typePartDefinition, partFieldDefinition, context);

            var result = await UpdateAsync(field, context.Updater, updateFieldEditorContext);

            if (result == null)
            {
                return null;
            }

            if (context.Updater.ModelState.IsValid)
            {
                contentPart.Apply(partFieldDefinition.Name, field);
            }

            return result;
        }

        public virtual Task<IDisplayResult> DisplayAsync(TField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return Task.FromResult(Display(field, fieldDisplayContext));
        }

        public virtual Task<IDisplayResult> EditAsync(TField field, BuildFieldEditorContext context)
        {
            return Task.FromResult(Edit(field, context));
        }

        public virtual Task<IDisplayResult> UpdateAsync(TField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            return Task.FromResult(Update(field, updater, context));
        }

        public virtual IDisplayResult Display(TField field, BuildFieldDisplayContext fieldDisplayContext)
        {
            return null;
        }

        public virtual IDisplayResult Edit(TField field, BuildFieldEditorContext context)
        {
            return null;
        }

        public virtual IDisplayResult Update(TField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            return null;
        }

    }
}
