using System.Collections.Generic;
using System.Linq;
using Orchard.ContentManagement.MetaData.Builders;
using Orchard.ContentManagement.MetaData.Models;
using Orchard.ContentManagement.ViewModels;

using Orchard.DisplayManagement.ModelBinding;
using Orchard.DependencyInjection;

namespace Orchard.ContentTypes.Editors
{
    public interface IContentDefinitionEditorEvents : IDependency
    {
        IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition);
        IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition);
        IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition);
        IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition);


        void TypeEditorUpdating(ContentTypeDefinitionBuilder builder);
        IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel);
        void TypeEditorUpdated(ContentTypeDefinitionBuilder builder);
        void TypePartEditorUpdating(ContentTypePartDefinitionBuilder builder);
        IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel);
        void TypePartEditorUpdated(ContentTypePartDefinitionBuilder builder);
        void PartEditorUpdating(ContentPartDefinitionBuilder builder);
        IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel);
        void PartEditorUpdated(ContentPartDefinitionBuilder builder);
        void PartFieldEditorUpdating(ContentPartFieldDefinitionBuilder builder);
        IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel);
        void PartFieldEditorUpdated(ContentPartFieldDefinitionBuilder builder);
    }

    public abstract class ContentDefinitionEditorEventsBase : IContentDefinitionEditorEvents
    {
        public virtual IEnumerable<TemplateViewModel> TypeEditor(ContentTypeDefinition definition)
        {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual IEnumerable<TemplateViewModel> TypePartEditor(ContentTypePartDefinition definition)
        {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual IEnumerable<TemplateViewModel> PartEditor(ContentPartDefinition definition)
        {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual IEnumerable<TemplateViewModel> PartFieldEditor(ContentPartFieldDefinition definition)
        {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual void TypeEditorUpdating(ContentTypeDefinitionBuilder builder) { }

        public virtual IEnumerable<TemplateViewModel> TypeEditorUpdate(ContentTypeDefinitionBuilder builder, IUpdateModel updateModel)
        {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual void TypeEditorUpdated(ContentTypeDefinitionBuilder builder) { }

        public virtual void TypePartEditorUpdating(ContentTypePartDefinitionBuilder builder) { }

        public virtual IEnumerable<TemplateViewModel> TypePartEditorUpdate(ContentTypePartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual void TypePartEditorUpdated(ContentTypePartDefinitionBuilder builder) { }

        public virtual void PartEditorUpdating(ContentPartDefinitionBuilder builder) { }

        public virtual IEnumerable<TemplateViewModel> PartEditorUpdate(ContentPartDefinitionBuilder builder, IUpdateModel updateModel)
        {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual void PartEditorUpdated(ContentPartDefinitionBuilder builder) { }

        public virtual void PartFieldEditorUpdating(ContentPartFieldDefinitionBuilder builder) { }

        public virtual IEnumerable<TemplateViewModel> PartFieldEditorUpdate(ContentPartFieldDefinitionBuilder builder, IUpdateModel updateModel)
        {
            return Enumerable.Empty<TemplateViewModel>();
        }

        public virtual void PartFieldEditorUpdated(ContentPartFieldDefinitionBuilder builder) { }

        protected static TemplateViewModel DefinitionTemplate<TModel>(TModel model)
        {
            return DefinitionTemplate(model, typeof(TModel).Name, typeof(TModel).Name);
        }

        protected static TemplateViewModel DefinitionTemplate<TModel>(TModel model, string templateName, string prefix)
        {
            return new TemplateViewModel(model, prefix)
            {
                TemplateName = "DefinitionTemplates/" + templateName
            };
        }
    }
}