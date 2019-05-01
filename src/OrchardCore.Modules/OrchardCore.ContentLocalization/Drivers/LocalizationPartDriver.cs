using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Services;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Settings;

namespace OrchardCore.ContentLocalization.Drivers {
    public class LocalizationPartDisplayDriver : ContentPartDisplayDriver<LocalizationPart> {
        private readonly ILocalizationPartViewModelBuilder _viewModelBuilder;

        public LocalizationPartDisplayDriver(
            ILocalizationPartViewModelBuilder viewModelBuilder
        ) {
            _viewModelBuilder = viewModelBuilder;

        }

        public override IDisplayResult Edit(LocalizationPart localizationPart) {

            return Initialize<LocalizationPartViewModel>("LocalizationPart_Edit", m => _viewModelBuilder.BuildViewModelAsync(m, localizationPart));
        }

        public override async Task<IDisplayResult> UpdateAsync(LocalizationPart model, IUpdateModel updater, UpdatePartEditorContext context) {
            var viewModel = new LocalizationPartViewModel();

            await updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Culture);

            model.Culture = viewModel.Culture;

            return Edit(model, context);
        }

    }
}