using System.Globalization;
using Microsoft.AspNetCore.Localization;
using OrchardCore.Admin.Models;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.ContentLocalization.Drivers;

public sealed class ContentCulturePickerNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly ILocalizationService _localizationService;

    public ContentCulturePickerNavbarDisplayDriver(ILocalizationService localizationService)
    {
        _localizationService = localizationService;
    }

    public override async Task<IDisplayResult> DisplayAsync(Navbar model, BuildDisplayContext context)
    {
        var supportedCultures = (await _localizationService.GetSupportedCulturesAsync()).Select(c => CultureInfo.GetCultureInfo(c));

        return Initialize<CulturePickerViewModel>("ContentCulturePicker", model =>
        {
            model.SupportedCultures = supportedCultures;
            model.CurrentCulture = context
            .HttpContext
            .Features
            .Get<IRequestCultureFeature>()?.RequestCulture?.Culture ?? CultureInfo.CurrentUICulture;
        }).RenderWhen(() => Task.FromResult(supportedCultures.Count() > 1))
        .Location(OrchardCoreConstants.DisplayType.Detail, "Content:5");
    }
}
