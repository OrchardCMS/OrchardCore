using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Distributed.Settings;
using OrchardCore.Distributed.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Distributed.Drivers
{
    public class RedisSiteSettingsDisplayDriver : SectionDisplayDriver<ISite, RedisSettings>
    {
        public override IDisplayResult Edit(RedisSettings section, BuildEditorContext context)
        {
            return Initialize<RedisSettingsViewModel>("RedisSettings_Edit", model =>
                {
                    model.Configuration = section.Configuration;
                }).Location("Content:2").OnGroup("redis");
        }

        public override async Task<IDisplayResult> UpdateAsync(RedisSettings section,  BuildEditorContext context)
        {
            if (context.GroupId == "redis")
            {
                var model = new RedisSettingsViewModel();

                await context.Updater.TryUpdateModelAsync(model, Prefix);

                section.Configuration = model.Configuration;
            }

            return await EditAsync(section, context);
        }
    }
}
