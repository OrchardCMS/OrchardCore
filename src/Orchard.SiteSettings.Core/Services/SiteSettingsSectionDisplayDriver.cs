using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Orchard.DisplayManagement.Handlers;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;

namespace Orchard.Settings.Services
{
    /// <summary>
    /// A concrete implementation of this class will be able to take part in the rendering of the site settings shape
    /// for a specific section of the configuration.
    /// </summary>
    /// <typeparam name="TSection"></typeparam>
    public abstract class SiteSettingsSectionDisplayDriver<TSection> : DisplayDriverBase, ISiteSettingsDisplayDriver where TSection : new()
    {
        Task<IDisplayResult> ISiteSettingsDisplayDriver.BuildEditorAsync(ISite site, BuildEditorContext context)
        {
            JToken property;
            TSection section;

            var typeName = typeof(TSection).Name;

            if (!site.Properties.TryGetValue(typeName, out property))
            {
                section = new TSection();
            }
            else
            {
                section = property.ToObject<TSection>();
            }            

            return EditAsync(section, context);
        }

        Task<IDisplayResult> ISiteSettingsDisplayDriver.UpdateEditorAsync(ISite site, UpdateEditorContext context)
        {
            JToken property;
            TSection section;

            var typeName = typeof(TSection).Name;

            if (!site.Properties.TryGetValue(typeName, out property))
            {
                section = new TSection();
            }
            else
            {
                section = property.ToObject<TSection>();
            }
            
            var result = UpdateAsync(section, context.Updater, context);

            if (result == null)
            {
                return Task.FromResult<IDisplayResult>(null);
            }

            if (context.Updater.ModelState.IsValid)
            {
                site.Properties[typeName] = JObject.FromObject(section);
            }

            return result;
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

    }
}
