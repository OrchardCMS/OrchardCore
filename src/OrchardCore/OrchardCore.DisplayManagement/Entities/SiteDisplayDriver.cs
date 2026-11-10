using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Entities;

public abstract class SiteDisplayDriver<TSettings> : SectionDisplayDriverBase<ISite, TSettings>
    where TSettings : new()
{
    protected abstract string SettingsGroupId { get; }

    public sealed override Task<IDisplayResult> DisplayAsync(ISite site, BuildDisplayContext context)
    {
        if (!string.Equals(SettingsGroupId, context.GroupId, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        return base.DisplayAsync(site, context);
    }

    public sealed override Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
    {
        if (!string.Equals(SettingsGroupId, context.GroupId, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        return base.EditAsync(site, context);
    }

    public sealed override Task<IDisplayResult> UpdateAsync(ISite site, UpdateEditorContext context)
    {
        if (!string.Equals(SettingsGroupId, context.GroupId, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult<IDisplayResult>(null);
        }

        return base.UpdateAsync(site, context);
    }

    public sealed override IDisplayResult Display(ISite site, BuildDisplayContext context)
        => base.Display(site, context);

    public sealed override IDisplayResult Edit(ISite site, BuildEditorContext context)
        => base.Edit(site, context);

    public sealed override ShapeResult Factory<TBuilderState, TInitState>(string shapeType, Func<IBuildShapeContext, TBuilderState, ValueTask<IShape>> shapeBuilder, TBuilderState shapeBuilderState, Func<IShape, TInitState, ValueTask> initializingAsync, TInitState initializingState)
        => base.Factory(shapeType, shapeBuilder, shapeBuilderState, initializingAsync, initializingState);

    public sealed override bool CanHandleModel(ISite model)
        => base.CanHandleModel(model);

    protected sealed override void BuildPrefix(ISite model, string htmlFieldPrefix)
        => base.BuildPrefix(model, htmlFieldPrefix);

    protected sealed override string PropertyName
        => base.PropertyName;
}
