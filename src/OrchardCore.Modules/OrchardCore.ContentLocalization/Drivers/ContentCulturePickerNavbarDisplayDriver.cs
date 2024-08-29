using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using OrchardCore.Admin.Models;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;

namespace OrchardCore.ContentLocalization.Drivers;

public sealed class ContentCulturePickerNavbarDisplayDriver : DisplayDriver<Navbar>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILocalizationService _localizationService;

    public ContentCulturePickerNavbarDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService)
    {
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
    }

    public override async Task<IDisplayResult> DisplayAsync(Navbar model, BuildDisplayContext context)
    {
        var supportedCultures = (await _localizationService.GetSupportedCulturesAsync()).Select(c => CultureInfo.GetCultureInfo(c));

        return Initialize<CulturePickerViewModel>("ContentCulturePicker", model =>
        {
            model.SupportedCultures = supportedCultures;
            model.CurrentCulture = _httpContextAccessor
            .HttpContext
            .Features
            .Get<IRequestCultureFeature>()?.RequestCulture?.Culture ?? CultureInfo.CurrentUICulture;
        }).RenderWhen(() => Task.FromResult(supportedCultures.Count() > 1))
        .Location("Detail", "Content:5");
    }
}
