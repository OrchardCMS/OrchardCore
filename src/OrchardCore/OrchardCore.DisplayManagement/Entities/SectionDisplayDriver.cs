using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;

namespace OrchardCore.DisplayManagement.Entities
{
    /// <summary>
    /// A concrete implementation of this class will be able to take part in the rendering of an <see cref="IEntity"/>
    /// shape instance for a specific section of the object. A section represents a property of an entity instance
    /// where the name of the property is the type of the section.
    /// </summary>
    /// <typeparam name="TModel">The type of model this driver handles.</typeparam>
    /// <typeparam name="TSection">The type of the section this driver handles.</typeparam>
    public abstract class SectionDisplayDriver<TModel, TSection> : DisplayDriver<TModel>
        where TSection : new()
        where TModel : class, IEntity
    {
        /// <summary>
        /// Gets the property name that the <typeparamref name="TSection"/> is stored on the <typeparamref name="TModel"/>.
        /// </summary>
        /// <remarks>
        /// Overriding this property allows changing the name of the property that the section is stored in from the
        /// default, which is <c>typeof(TSection).Name</c>.
        /// </remarks>
        protected virtual string PropertyName => typeof(TSection).Name;

        public override Task<IDisplayResult> DisplayAsync(TModel model, BuildDisplayContext context)
        {
            var section = GetSection(model);

            return DisplayAsync(model, section, context);
        }

        public override Task<IDisplayResult> EditAsync(TModel model, BuildEditorContext context)
        {
            var section = GetSection(model);

            return EditAsync(model, section, context);
        }

        public override async Task<IDisplayResult> UpdateAsync(TModel model, UpdateEditorContext context)
        {
            var section = GetSection(model);

            var result = await UpdateAsync(model, section, context.Updater, context);

            if (result == null)
            {
                return null;
            }

            if (context.Updater.ModelState.IsValid)
            {
                model.Properties[PropertyName] = JObject.FromObject(section);
            }

            return result;
        }

        public virtual Task<IDisplayResult> DisplayAsync(TModel model, TSection section, BuildDisplayContext context)
        {
            return DisplayAsync(section, context);
        }

        public virtual Task<IDisplayResult> DisplayAsync(TSection section, BuildDisplayContext context)
        {
            return Task.FromResult(Display(section, context));
        }

        public virtual IDisplayResult Display(TSection section, BuildDisplayContext context)
        {
            return Display(section);
        }

        public virtual IDisplayResult Display(TSection section)
        {
            return null;
        }

        public virtual Task<IDisplayResult> EditAsync(TModel model, TSection section, BuildEditorContext context)
        {
            return EditAsync(section, context);
        }

        public virtual Task<IDisplayResult> EditAsync(TSection section, BuildEditorContext context)
        {
            return Task.FromResult(Edit(section, context));
        }

        public virtual IDisplayResult Edit(TSection section, BuildEditorContext context)
        {
            return Edit(section);
        }

        public virtual IDisplayResult Edit(TSection section)
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(TModel model, TSection section, IUpdateModel updater, BuildEditorContext context)
        {
            return UpdateAsync(section, updater, context);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TSection section, IUpdateModel updater, BuildEditorContext context)
        {
            return UpdateAsync(section, context);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TSection section, BuildEditorContext context)
        {
            return UpdateAsync(section, context.Updater, context.GroupId);
        }

        public virtual Task<IDisplayResult> UpdateAsync(TSection section, IUpdateModel updater, string groupId)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        private TSection GetSection(TModel model)
        {
            return model.Properties.TryGetValue(PropertyName, out var property)
                ? property.ToObject<TSection>()
                : new TSection();
        }
    }
}
