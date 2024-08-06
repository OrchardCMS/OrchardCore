using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
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
        => throw new NotImplementedException();

    public sealed override IDisplayResult Edit(ISite site, BuildEditorContext context)
        => throw new NotImplementedException();

#pragma warning disable CS0672 // Member overrides obsolete member
    public sealed override IDisplayResult Display(ISite site)
        => throw new NotImplementedException();

    public sealed override IDisplayResult Display(TSettings settings)
        => throw new NotImplementedException();

    public sealed override IDisplayResult Display(TSettings settings, BuildDisplayContext context)
        => throw new NotImplementedException();

    public sealed override IDisplayResult Edit(TSettings settings, BuildEditorContext context)
        => throw new NotImplementedException();

    public sealed override IDisplayResult Edit(ISite model)
        => throw new NotImplementedException();

    public sealed override IDisplayResult Edit(TSettings settings)
        => throw new NotImplementedException();

    public sealed override Task<IDisplayResult> UpdateAsync(TSettings settings, UpdateEditorContext context)
        => throw new NotImplementedException();

    public sealed override Task<IDisplayResult> UpdateAsync(TSettings settings, IUpdateModel updater, string groupId)
        => throw new NotImplementedException();
#pragma warning restore CS0672 // Member overrides obsolete member
}
