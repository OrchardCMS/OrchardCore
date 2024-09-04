using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;

namespace OrchardCore.DisplayManagement.Entities;

/// <summary>
/// A concrete implementation of this class will be able to take part in the rendering of an <see cref="IEntity"/>
/// shape instance for a specific section of the object. A section represents a property of an entity instance
/// where the name of the property is the type of the section.
/// </summary>
/// <typeparam name="TModel">The type of model this driver handles.</typeparam>
/// <typeparam name="TSection">The type of the section this driver handles.</typeparam>
public abstract class SectionDisplayDriver<TModel, TSection> : SectionDisplayDriverBase<TModel, TSection>
    where TSection : new()
    where TModel : class, IEntity
{
    public sealed override Task<IDisplayResult> DisplayAsync(TModel model, BuildDisplayContext context)
        => base.DisplayAsync(model, context);

    public override IDisplayResult Display(TModel model, TSection section, BuildDisplayContext context)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return Display(section, context);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the DisplayAsync(TModel model, TSection section, BuildDisplayContext context) ")]
    public virtual IDisplayResult Display(TSection section, BuildDisplayContext context)
    {
        return Display(section);
    }

    [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the DisplayAsync(TModel model, TSection section, BuildDisplayContext context) ")]
    public virtual IDisplayResult Display(TSection section)
        => NullShapeResult();

    public sealed override Task<IDisplayResult> EditAsync(TModel model, BuildEditorContext context)
        => base.EditAsync(model, context);

    public override IDisplayResult Edit(TModel model, TSection section, BuildEditorContext context)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return Edit(section, context);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the EditAsync(TModel model, TSection section, BuildEditorContext context) or Edit(TModel model, TSection section, BuildEditorContext context)")]
    public virtual IDisplayResult Edit(TSection section, BuildEditorContext context)
        => Edit(section);

    [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the EditAsync(TModel model, TSection section, BuildEditorContext context) or Edit(TModel model, TSection section, BuildEditorContext context)")]
    public virtual IDisplayResult Edit(TSection section)
        => NullShapeResult();

    public sealed override Task<IDisplayResult> UpdateAsync(TModel model, UpdateEditorContext context)
        => base.UpdateAsync(model, context);

    public override Task<IDisplayResult> UpdateAsync(TModel model, TSection section, UpdateEditorContext context)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        return UpdateAsync(section, context);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the UpdateAsync(TModel model, TSection section, UpdateEditorContext context)")]
    public virtual Task<IDisplayResult> UpdateAsync(TSection section, UpdateEditorContext context)
        => UpdateAsync(section, context.Updater, context.GroupId);

    [Obsolete("This method is obsolete and will be removed in version 3. Instead, use the UpdateAsync(TModel model, TSection section, UpdateEditorContext context)")]
    public virtual Task<IDisplayResult> UpdateAsync(TSection section, IUpdateModel updater, string groupId)
        => Task.FromResult(NullShapeResult());
}
