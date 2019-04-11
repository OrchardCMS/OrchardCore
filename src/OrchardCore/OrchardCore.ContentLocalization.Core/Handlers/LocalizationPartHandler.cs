using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;

using OrchardCore.Entities;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Handlers
{
    public class LocalizationPartHandler : ContentPartHandler<LocalizationPart>
    {
        private readonly IIdGenerator generator;
        private readonly ISiteService siteService;

        public LocalizationPartHandler(IIdGenerator generator, ISiteService siteService)
        {
            this.generator = generator;
            this.siteService = siteService;
        }

        public override async Task InitializingAsync(InitializingContentContext context, LocalizationPart instance)
        {
            var setting = await siteService.GetSiteSettingsAsync();
            instance.Culture = setting.Culture;
            instance.Apply();
            await base.InitializingAsync(context, instance);
        }

        public override Task CreatingAsync(CreateContentContext context, LocalizationPart instance)
        {
            instance.LocalizationSet = generator.GenerateUniqueId();
            instance.Apply();
            return base.CreatingAsync(context, instance);
        }
        public override async Task UpdatingAsync(UpdateContentContext context, LocalizationPart instance)
        {
            if( instance.LocalizationSet == null)
            {
                instance.LocalizationSet = generator.GenerateUniqueId();
            }

            if(instance.Culture == null)
            {
                var setting = await siteService.GetSiteSettingsAsync();
                instance.Culture = setting.Culture;
            }
            instance.Apply();
            await base.UpdatingAsync(context, instance);
        }
        public override Task RemovingAsync(RemoveContentContext context, LocalizationPart instance)
        {
            return base.RemovingAsync(context, instance);
        }
    }
}
