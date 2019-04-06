using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentLocalization
{
    public class SummaryAdminDriver : ContentDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public SummaryAdminDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(ContentItem model, IUpdateModel updater)
        {
            var localizationPart = model.As<LocalizationPart>();
            if (localizationPart != null)
            {
                // This injects a button on the SummaryAdmin view for the Hackathon ContentType
                return Shape("LocalizationPart_SummaryAdmin", new ContentItemViewModel(model)).Location("SummaryAdmin", "Content:after");
            }
            return null;
        }
    }
}
