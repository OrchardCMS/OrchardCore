using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Users.Localization.Models;
using OrchardCore.Users.Localization.ViewModels;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Localization.Drivers;

public class UserLocalizationDisplayDriver : SectionDisplayDriver<User, UserLocalizationSettings>
{
    private readonly ILocalizationService _localizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserLocalizationDisplayDriver(
        ILocalizationService localizationService,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _localizationService = localizationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<IDisplayResult> EditAsync(UserLocalizationSettings section, BuildEditorContext context)
    {
        return Task.FromResult<IDisplayResult>(Initialize<UserLocalizationViewModel>("UserCulture_Edit", async model =>
        {
            var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
            var currentUserCulture = _httpContextAccessor.HttpContext.User.FindFirstValue("culture");

            model.Culture = section.Culture;
            model.SupportedCultures = supportedCultures.Select(culture =>
            new SelectListItem()
            {
                Text = CultureInfo.GetCultureInfo(culture).DisplayName + " (" + culture + ")",
                Value = culture,
                Selected = currentUserCulture == culture
            });
        }).Location("Content:2"));
    }

    public override async Task<IDisplayResult> UpdateAsync(User model, UserLocalizationSettings section, IUpdateModel updater, BuildEditorContext context)
    {
        var viewModel = new UserLocalizationViewModel();

        if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            section.Culture = viewModel.Culture;
        }

        return await EditAsync(section, context);
    }
}
