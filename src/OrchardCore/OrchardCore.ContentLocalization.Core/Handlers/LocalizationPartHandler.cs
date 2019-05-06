using System;
using System.Globalization;
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

        /// <summary>
        /// If we create a new content item then we initialize it with the current default site culture.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public override async Task InitializingAsync(InitializingContentContext context, LocalizationPart instance)
        {
            if (instance.Culture == null)
            {
                instance.Culture = await GetDefaultCultureNameAsync();
                instance.Apply();
            }

            await base.InitializingAsync(context, instance);
        }
        /// <summary>
        /// If we are loading a content item that has no LocalizationPart then we set it's culture the default site culture
        /// </summary>
        public override async Task LoadingAsync(LoadContentContext context, LocalizationPart instance)
        {
            if (instance.Culture == null)
            {
                instance.Culture = await GetDefaultCultureNameAsync();
                instance.Apply();
            }

            await base.LoadingAsync(context, instance);
        }

        /// <summary>
        /// If we are updating a content item that has no LocalizationPart
        /// then we generate a unique LocalizationSet
        /// </summary>
        public override async Task UpdatingAsync(UpdateContentContext context, LocalizationPart instance)
        {
            if (instance.LocalizationSet == null)
            {
                instance.LocalizationSet = generator.GenerateUniqueId();
            }

            if (instance.Culture == null)
            {
                instance.Culture = await GetDefaultCultureNameAsync();
            }

            instance.Apply();
            await base.UpdatingAsync(context, instance);
        }

        private async Task<string> GetDefaultCultureNameAsync()
        {
            var setting = await siteService.GetSiteSettingsAsync();

            if (!String.IsNullOrEmpty(setting.Culture))
            {
                return CultureInfo.GetCultureInfo(setting.Culture).Name;
            }

            return CultureInfo.InstalledUICulture.Name;
        }
    }
}
