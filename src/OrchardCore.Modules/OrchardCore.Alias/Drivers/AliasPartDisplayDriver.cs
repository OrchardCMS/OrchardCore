using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Settings;
using OrchardCore.Alias.ViewModels;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using YesSql;

namespace OrchardCore.Alias.Drivers
{
    public class AliasPartDisplayDriver : ContentPartDisplayDriver<AliasPart>
    {

        private readonly ISession _session;
        protected readonly IStringLocalizer S;

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
            return Initialize<AliasPartViewModel>(GetEditorShapeType(context), m => BuildViewModel(m, aliasPart, context.TypePartDefinition.GetSettings<AliasPartSettings>()));
        }

        public override async Task<IDisplayResult> UpdateAsync(AliasPart model, IUpdateModel updater, UpdatePartEditorContext context)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Alias);

            await foreach (var item in model.ValidateAsync(S, _session))
            {
                updater.ModelState.BindValidationResult(Prefix, item);
            }

            return Edit(model, context);
        }

        private static void BuildViewModel(AliasPartViewModel model, AliasPart part, AliasPartSettings settings)
        {
            model.Alias = part.Alias;
            model.AliasPart = part;
            model.ContentItem = part.ContentItem;
            model.Settings = settings;
        }

    }
}
