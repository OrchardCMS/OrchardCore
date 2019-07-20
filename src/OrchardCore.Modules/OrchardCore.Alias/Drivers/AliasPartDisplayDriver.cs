using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Indexes;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Settings;
using OrchardCore.Alias.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using YesSql;

namespace OrchardCore.Alias.Drivers
{
    public class AliasPartDisplayDriver : ContentPartDisplayDriver<AliasPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly IStringLocalizer<AliasPartDisplayDriver> T;

        public AliasPartDisplayDriver(
            IContentDefinitionManager contentDefinitionManager,
            ISession session,
            IStringLocalizer<AliasPartDisplayDriver> localizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            T = localizer;
        }

        public override IDisplayResult Edit(AliasPart aliasPart)
        {
            return Initialize<AliasPartViewModel>("AliasPart_Edit", m => BuildViewModelAsync(m, aliasPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(AliasPart model, IUpdateModel updater)
        {
            var settings = await GetAliasPartSettingsAsync(model);

            await updater.TryUpdateModelAsync(model, Prefix, t => t.Alias);

            await ValidateAsync(model, updater);

            return Edit(model);
        }

        public async Task<AliasPartSettings> GetAliasPartSettingsAsync(AliasPart part)
        {
            var contentTypeDefinition = await _contentDefinitionManager.GetTypeDefinitionAsync(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(AliasPart));
            var settings = contentTypePartDefinition.GetSettings<AliasPartSettings>();

            return settings;
        }

        private async Task BuildViewModelAsync(AliasPartViewModel model, AliasPart part)
        {
            var settings = await GetAliasPartSettingsAsync(part);

            model.Alias = part.Alias;
            model.AliasPart = part;
            model.Settings = settings;
        }

        private async Task ValidateAsync(AliasPart alias, IUpdateModel updater)
        {
            if (alias.Alias != null && (await _session.QueryIndex<AliasPartIndex>(o => o.Alias == alias.Alias && o.ContentItemId != alias.ContentItem.ContentItemId).CountAsync()) > 0)
            {
                updater.ModelState.AddModelError(Prefix, nameof(alias.Alias), T["Your alias is already in use."]);
            }
        }
    }
}
