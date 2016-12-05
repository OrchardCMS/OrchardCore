using System;
using System.Linq;
using System.Threading.Tasks;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.MetaData;
using Orchard.DisplayManagement.ModelBinding;
using Orchard.DisplayManagement.Views;
using Orchard.Identity.Models;
using Orchard.Identity.Settings;
using Orchard.Identity.ViewModels;

namespace Orchard.Identity.Drivers
{
    public class IdentityPartDisplay : ContentPartDisplayDriver<IdentityPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public IdentityPartDisplay(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Edit(IdentityPart identityPart)
        {
            return Shape<IdentityPartViewModel>("IdentityPart_Edit", m => BuildViewModel(m, identityPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(IdentityPart model, IUpdateModel updater)
        {
            var settings = GetIdentityPartSettings(model);

            if (settings.ShowNameEditor)
            {
                await updater.TryUpdateModelAsync(model, Prefix, t => t.Name);
            }

            var canChangeIdentity = settings.AllowCustomIdentity
                //&& (String.IsNullOrEmpty(model.Identity) || settings.AllowChangeIdentity)
                ;
            if (canChangeIdentity)
            {
                await updater.TryUpdateModelAsync(model, Prefix, t => t.Identity);
            }

            return Edit(model);
        }

        public IdentityPartSettings GetIdentityPartSettings(IdentityPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(p => p.PartDefinition.Name == nameof(IdentityPart));
            var settings = contentTypePartDefinition.GetSettings<IdentityPartSettings>();

            return settings;
        }

        private Task BuildViewModel(IdentityPartViewModel model, IdentityPart part)
        {
            var settings = GetIdentityPartSettings(part);

            model.Identity = part.Identity;
            model.Name = part.Name;
            model.IdentityPart = part;
            model.IdentityPartSettings = settings;

            return Task.CompletedTask;
        }
    }
}
