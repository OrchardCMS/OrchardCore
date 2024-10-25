using System.Globalization;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Localization;

namespace OrchardCore.ContentLocalization.Drivers;

public sealed class LocalizationPartDisplayDriver : ContentPartDisplayDriver<LocalizationPart>
{
    private readonly IContentLocalizationManager _contentLocalizationManager;
    private readonly IIdGenerator _idGenerator;
    private readonly ILocalizationService _localizationService;

    public LocalizationPartDisplayDriver(
        IContentLocalizationManager contentLocalizationManager,
        IIdGenerator idGenerator,
        ILocalizationService localizationService
    )
    {
        _contentLocalizationManager = contentLocalizationManager;
        _idGenerator = idGenerator;
        _localizationService = localizationService;
    }

    public override IDisplayResult Display(LocalizationPart part, BuildPartDisplayContext context)
    {
        return Combine(
            Initialize<LocalizationPartViewModel>("LocalizationPart_SummaryAdmin", model => BuildViewModelAsync(model, part)).Location("SummaryAdmin", "Tags:11"),
            Initialize<LocalizationPartViewModel>("LocalizationPart_SummaryAdminLinks", model => BuildViewModelAsync(model, part)).Location("SummaryAdmin", "Actions:5")
        );
    }

    public override IDisplayResult Edit(LocalizationPart localizationPart, BuildPartEditorContext context)
    {
        return Initialize<LocalizationPartViewModel>(GetEditorShapeType(context), m => BuildViewModelAsync(m, localizationPart));
    }

    public override async Task<IDisplayResult> UpdateAsync(LocalizationPart model, UpdatePartEditorContext context)
    {
        var viewModel = new LocalizationPartViewModel();
        await context.Updater.TryUpdateModelAsync(viewModel, Prefix, t => t.Culture);

        // Invariant culture name is empty so a null value is bound.
        model.Culture = viewModel.Culture ?? string.Empty;

        // Need to do this here to support displaying the message to save before localizing when the item has not been saved yet.
        if (string.IsNullOrEmpty(model.LocalizationSet))
        {
            model.LocalizationSet = _idGenerator.GenerateUniqueId();
        }
        return Edit(model, context);
    }

    public async ValueTask BuildViewModelAsync(LocalizationPartViewModel model, LocalizationPart localizationPart)
    {
        var alreadyTranslated = await _contentLocalizationManager.GetItemsForSetAsync(localizationPart.LocalizationSet);

        model.Culture = localizationPart.Culture;
        model.LocalizationSet = localizationPart.LocalizationSet;
        model.LocalizationPart = localizationPart;

        // Invariant culture name is empty so we only do a null check.
        model.Culture ??= await _localizationService.GetDefaultCultureAsync();

        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
        var currentCultures = supportedCultures.Where(c => c != model.Culture).Select(culture =>
          {
              return new LocalizationLinksViewModel()
              {
                  IsDeleted = false,
                  Culture = CultureInfo.GetCultureInfo(culture),
                  ContentItemId = alreadyTranslated.FirstOrDefault(c => c.As<LocalizationPart>()?.Culture == culture)?.ContentItemId,
              };
          }).ToList();

        // Content items that have been translated but the culture was removed from the settings page
        var deletedCultureTranslations = alreadyTranslated.Where(c => c.As<LocalizationPart>()?.Culture != model.Culture).Select(ci =>
        {
            var culture = ci.As<LocalizationPart>()?.Culture;
            if (currentCultures.Any(c => c.ContentItemId == ci.ContentItemId) || culture == null)
            {
                return null;
            }
            return new LocalizationLinksViewModel()
            {
                IsDeleted = true,
                Culture = CultureInfo.GetCultureInfo(culture),
                ContentItemId = ci?.ContentItemId
            };
        }).OfType<LocalizationLinksViewModel>().ToList();

        model.ContentItemCultures = currentCultures.Concat(deletedCultureTranslations).ToList();
    }
}
