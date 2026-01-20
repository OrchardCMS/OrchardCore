using OrchardCore.DisplayManagement.Handlers;
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
        => NullShapeResult();

    public sealed override Task<IDisplayResult> EditAsync(TModel model, BuildEditorContext context)
        => base.EditAsync(model, context);

    public override IDisplayResult Edit(TModel model, TSection section, BuildEditorContext context)
        => NullShapeResult();

    public sealed override Task<IDisplayResult> UpdateAsync(TModel model, UpdateEditorContext context)
        => base.UpdateAsync(model, context);

    public override Task<IDisplayResult> UpdateAsync(TModel model, TSection section, UpdateEditorContext context)
        => Task.FromResult(NullShapeResult());
}
