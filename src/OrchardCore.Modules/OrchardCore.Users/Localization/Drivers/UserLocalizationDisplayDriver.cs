using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
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
    protected readonly IStringLocalizer S;

    public UserLocalizationDisplayDriver(
        ILocalizationService localizationService,
        IStringLocalizer<UserLocalizationDisplayDriver> localizer)
    {
        _localizationService = localizationService;
        S = localizer;
    }

    public override Task<IDisplayResult> EditAsync(UserLocalizationSettings section, BuildEditorContext context)
    {
        return Task.FromResult<IDisplayResult>(Initialize<UserLocalizationViewModel>("UserCulture_Edit", async model =>
        {
            var supportedCultures = await _localizationService.GetSupportedCulturesAsync();

            var cultureList = supportedCultures.Select(culture =>
                new SelectListItem
                {
                    Text = CultureInfo.GetCultureInfo(culture).DisplayName + " (" + culture + ")",
                    Value = culture
                }).ToList();

            cultureList.Insert(0, new SelectListItem() { Text = S["Use site's culture"], Value = "none" });

            // If Invariant Culture is installed as a supported culture we bind it to a different culture code than String.Empty.
            var emptyCulture = cultureList.FirstOrDefault(c => c.Value == "");
            if (emptyCulture != null)
            {
                emptyCulture.Value = UserLocalizationConstants.Invariant;
            }

            model.SelectedCulture = section.Culture;
            model.CultureList = cultureList;
        }).Location("Content:2"));
    }

    public override async Task<IDisplayResult> UpdateAsync(User model, UserLocalizationSettings section, IUpdateModel updater, BuildEditorContext context)
    {
        var viewModel = new UserLocalizationViewModel();

        if (await context.Updater.TryUpdateModelAsync(viewModel, Prefix))
        {
            section.Culture = viewModel.SelectedCulture;
        }

        return await EditAsync(section, context);
    }
}
