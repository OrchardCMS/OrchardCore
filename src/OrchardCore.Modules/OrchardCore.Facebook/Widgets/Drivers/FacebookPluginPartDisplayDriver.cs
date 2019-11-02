using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Facebook.Widgets.Models;
using OrchardCore.Facebook.Widgets.Settings;
using OrchardCore.Facebook.Widgets.ViewModels;

namespace OrchardCore.Facebook.Widgets.Drivers
{
    public class FacebookPluginPartDisplayDriver : ContentPartDisplayDriver<FacebookPluginPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public FacebookPluginPartDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(FacebookPluginPart part)
        {
            return Combine(
                Initialize<FacebookPluginPartViewModel>("FacebookPluginPart", m => BuildViewModel(m, part))
                    .Location("Detail", "Content:10"),
                Initialize<FacebookPluginPartViewModel>("FacebookPluginPart_Summary", m => BuildViewModel(m, part))
                    .Location("Summary", "Content:10")
            );
        }

        private void BuildViewModel(FacebookPluginPartViewModel model, FacebookPluginPart part)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            model.FacebookPluginPart = part ?? throw new ArgumentNullException(nameof(part));
            model.Settings = GetFacebookPluginPartSettings(part);
            model.Liquid = part.Liquid;
            model.ContentItem = part.ContentItem;
        }

        public override IDisplayResult Edit(FacebookPluginPart part)
        {
            return Initialize<FacebookPluginPartViewModel>("FacebookPluginPart_Edit", model =>
            {
                model.Settings = GetFacebookPluginPartSettings(part);
                model.FacebookPluginPart = part;
                model.Liquid = string.IsNullOrWhiteSpace(part.Liquid) ? model.Settings.Liquid : part.Liquid;
            });
        }

        private FacebookPluginPartSettings GetFacebookPluginPartSettings(FacebookPluginPart part)
        {
            if (part == null)
            {
                throw new ArgumentNullException(nameof(part));
            }

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(FacebookPluginPart), StringComparison.Ordinal));
            return contentTypePartDefinition.GetSettings<FacebookPluginPartSettings>();
        }

        public override async Task<IDisplayResult> UpdateAsync(FacebookPluginPart model, IUpdateModel updater)
        {
            await updater.TryUpdateModelAsync(model, Prefix, t => t.Liquid);

            return Edit(model);
        }
    }
}
