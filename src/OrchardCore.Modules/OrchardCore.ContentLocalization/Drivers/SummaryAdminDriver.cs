using System.Threading.Tasks;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.ContentLocalization {
    public class SummaryAdminDriver : ContentDisplayDriver {

        private readonly ILocalizationPartViewModelBuilder _viewModelBuilder;

        public SummaryAdminDriver(
            ILocalizationPartViewModelBuilder viewModelBuilder
        ) {
            _viewModelBuilder = viewModelBuilder;

        }
        public override async Task<IDisplayResult> DisplayAsync(ContentItem model, IUpdateModel updater) {
            var localizationPart = model.As<LocalizationPart>();
            if (localizationPart != null) {
                var vm = new LocalizationPartViewModel();
                await _viewModelBuilder.BuildViewModelAsync(vm, localizationPart);

                return Combine(
                    Shape("LocalizationPart_SummaryAdmin", new ContentItemViewModel(model)).Location("SummaryAdmin", "Meta:11"),
                    Shape("LocalizationPart_SummaryAdminLinks", vm).Location("SummaryAdmin", "Actions:5")
                );
            }
            return null;
        }
    }
}