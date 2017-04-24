using System.Threading.Tasks;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// A concrete implementation of this class will be able to take part in the rendering of the site settings.
    /// </summary>
    public abstract class SiteSettingsDisplayDriver : DisplayDriverBase, ISiteSettingsDisplayDriver
    {
        Task<IDisplayResult> ISiteSettingsDisplayDriver.BuildEditorAsync(ISite site, BuildEditorContext context)
        {
            return EditAsync(site, context);
        }

        Task<IDisplayResult> ISiteSettingsDisplayDriver.UpdateEditorAsync(ISite site, UpdateEditorContext context)
        {
            return UpdateAsync(site, context.Updater, context);
        }

        public virtual Task<IDisplayResult> EditAsync(ISite site, BuildEditorContext context)
        {
            return Task.FromResult(Edit(site, context));
        }

        public virtual IDisplayResult Edit(ISite site, BuildEditorContext context)
        {
            return Edit(site);
        }

        public virtual IDisplayResult Edit(ISite site)
        {
            return null;
        }

        public virtual Task<IDisplayResult> UpdateAsync(ISite site, IUpdateModel updater, BuildEditorContext context)
        {
            return UpdateAsync(site, context);
        }

        public virtual Task<IDisplayResult> UpdateAsync(ISite site, BuildEditorContext context)
        {
            return UpdateAsync(site, context.Updater, context.GroupId);
        }

        public virtual Task<IDisplayResult> UpdateAsync(ISite site, IUpdateModel updaterm, string groupId)
        {
            return Task.FromResult<IDisplayResult>(null);
        }

    }
}
