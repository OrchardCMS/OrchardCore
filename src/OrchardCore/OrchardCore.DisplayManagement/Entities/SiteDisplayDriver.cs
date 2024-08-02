using System;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.DisplayManagement.Entities
{
    public abstract class SiteDisplayDriver<TSection> : SectionDisplayDriver<ISite, TSection>
        where TSection : new()
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
    }
}
