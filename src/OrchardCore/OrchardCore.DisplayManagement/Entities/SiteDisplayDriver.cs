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

    public sealed override ShapeResult Factory(string shapeType, Func<IBuildShapeContext, ValueTask<IShape>> shapeBuilder, Func<IShape, Task> initializeAsync)
        => base.Factory(shapeType, shapeBuilder, initializeAsync);

    public sealed override bool CanHandleModel(ISite model)
        => base.CanHandleModel(model);

    protected sealed override void BuildPrefix(ISite model, string htmlFieldPrefix)
        => base.BuildPrefix(model, htmlFieldPrefix);

    protected sealed override string PropertyName
        => base.PropertyName;

#pragma warning disable CS0672 // Member overrides obsolete member
#pragma warning disable CS0618 // Type or member is obsolete
    public sealed override IDisplayResult Display(ISite site)
        => base.Display(site);

    public sealed override IDisplayResult Edit(ISite site)
        => base.Edit(site);
#pragma warning restore CS0672 // Member overrides obsolete member
#pragma warning restore CS0618 // Type or member is obsolete
}
