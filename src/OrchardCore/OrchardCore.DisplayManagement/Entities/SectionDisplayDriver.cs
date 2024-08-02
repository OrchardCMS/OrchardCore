using System;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
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
        /// Gets the property name that the <typeparamref name="TSection"/> is stored in the <typeparamref name="TModel"/>.
        /// </summary>
        /// <remarks>
        /// Overriding this property allows changing the name of the property that the section is stored in from the
        /// default, which is <c>typeof(TSection).Name</c>.
        /// </remarks>
        protected virtual string PropertyName
            => typeof(TSection).Name;

        public override Task<IDisplayResult> DisplayAsync(TModel model, BuildDisplayContext context)
        {
            var section = GetSection(model);

            return DisplayAsync(model, section, context);
        }

        public virtual Task<IDisplayResult> DisplayAsync(TModel model, TSection section, BuildDisplayContext context)
        {
            return Task.FromResult(Display(model, section, context));
        }

        public virtual IDisplayResult Display(TModel model, TSection section, BuildDisplayContext context)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Display(section, context);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete("This method will be removed. Instead use DisplayAsync(TModel model, TSection section, BuildDisplayContext context) ")]
        public virtual IDisplayResult Display(TSection section, BuildDisplayContext context)
        {
            return Display(section);
        }

        [Obsolete("This method will be removed. Instead use DisplayAsync(TModel model, TSection section, BuildDisplayContext context) ")]
        public virtual IDisplayResult Display(TSection section)
        {
            return null;
        }

        public override Task<IDisplayResult> EditAsync(TModel model, BuildEditorContext context)
        {
            var section = GetSection(model);

            return EditAsync(model, section, context);
        }

        public virtual Task<IDisplayResult> EditAsync(TModel model, TSection section, BuildEditorContext context)
        {
            return Task.FromResult(Edit(model, section, context));
        }

        public virtual IDisplayResult Edit(TModel model, TSection section, BuildEditorContext context)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return Edit(section, context);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete("This method will be removed. Instead use EditAsync(TModel model, TSection section, BuildEditorContext context) or Edit(TModel model, TSection section, BuildEditorContext context)")]
        public virtual IDisplayResult Edit(TSection section, BuildEditorContext context)
        {
            return Edit(section);
        }

        [Obsolete("This method will be removed. Instead use EditAsync(TModel model, TSection section, BuildEditorContext context) or Edit(TModel model, TSection section, BuildEditorContext context)")]
        public virtual IDisplayResult Edit(TSection section)
            => NullShapeResult();

        public override async Task<IDisplayResult> UpdateAsync(TModel model, UpdateEditorContext context)
        {
            var section = GetSection(model);

            var result = await UpdateAsync(model, section, context);

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

        public virtual Task<IDisplayResult> UpdateAsync(TModel model, TSection section, UpdateEditorContext context)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            return UpdateAsync(section, context);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        [Obsolete("This method will be removed. Instead use UpdateAsync(TModel model, TSection section, UpdateEditorContext context)")]
        public virtual Task<IDisplayResult> UpdateAsync(TSection section, UpdateEditorContext context)
        {
            return UpdateAsync(section, context.Updater, context.GroupId);
        }

        [Obsolete("This method will be removed. Instead use UpdateAsync(TModel model, TSection section, UpdateEditorContext context)")]
        public virtual Task<IDisplayResult> UpdateAsync(TSection section, IUpdateModel updater, string groupId)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        private TSection GetSection(TModel model)
            => model.Properties.TryGetPropertyValue(PropertyName, out var section)
            ? section.ToObject<TSection>()
            : new TSection();

        protected override void BuildPrefix(TModel model, string htmlFieldPrefix)
        {
            if (!string.IsNullOrEmpty(htmlFieldPrefix))
            {
                Prefix = $"{htmlFieldPrefix}.{ModelName}.{PropertyName}";
            }
            else
            {
                Prefix = $"{ModelName}.{PropertyName}";
            }
        }
    }
}
