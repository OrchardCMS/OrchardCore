using System.Threading.Tasks;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Settings;
using OrchardCore.Alias.ViewModels;
using YesSql;
using OrchardCore.Alias.Indexes;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.Mvc.ModelBinding;

namespace OrchardCore.Alias.Drivers
{
    public class AliasPartDisplayDriver : ContentPartDisplayDriver<AliasPart>
    {
        // Maximum length that MySql can support in an index under utf8 collation.
        public const int MaxAliasLength = 767;

        private readonly ISession _session;
        private readonly IStringLocalizer<AliasPartDisplayDriver> S;

        public AliasPartDisplayDriver(
            ISession session,
            IStringLocalizer<AliasPartDisplayDriver> localizer
        )
        {
            _session = session;
            S = localizer;
        }

        public override IDisplayResult Edit(AliasPart aliasPart, BuildPartEditorContext context)
        {
            return Initialize<AliasPartViewModel>("AliasPart_Edit", m => BuildViewModel(m, aliasPart, context.TypePartDefinition.GetSettings<AliasPartSettings>()));
        }

        public override async Task<IDisplayResult> UpdateAsync(AliasPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Alias);

            await ValidateAsync(model, updater);

            return Edit(model,context);
        }

        private void BuildViewModel(AliasPartViewModel model, AliasPart part, AliasPartSettings settings)
        {
            model.Alias = part.Alias;
            model.AliasPart = part;
            model.Settings = settings;
        }

        private async Task ValidateAsync(AliasPart alias, IUpdateModel updater)
        {
            if (alias.Alias?.Length > MaxAliasLength)
            {
                updater.ModelState.AddModelError(Prefix, nameof(alias.Alias), S["Your alias is too long. The alias can only be up to {0} characters.", MaxAliasLength]);
            }

            if (alias.Alias != null && (await _session.QueryIndex<AliasPartIndex>(o => o.Alias == alias.Alias && o.ContentItemId != alias.ContentItem.ContentItemId).CountAsync()) > 0)
            {
                updater.ModelState.AddModelError(Prefix, nameof(alias.Alias), S["Your alias is already in use."]);
            }
        }
    }
}
