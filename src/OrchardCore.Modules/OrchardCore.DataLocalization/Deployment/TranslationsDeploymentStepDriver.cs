using OrchardCore.DataLocalization.ViewModels;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Localization;
using OrchardCore.Localization.Data;

namespace OrchardCore.DataLocalization.Deployment;

public sealed class TranslationsDeploymentStepDriver : DisplayDriver<DeploymentStep, TranslationsDeploymentStep>
{
    private readonly ILocalizationService _localizationService;
    private readonly IEnumerable<ILocalizationDataProvider> _localizationDataProviders;

    public TranslationsDeploymentStepDriver(
        ILocalizationService localizationService,
        IEnumerable<ILocalizationDataProvider> localizationDataProviders)
    {
        _localizationService = localizationService;
        _localizationDataProviders = localizationDataProviders;
    }

    public override Task<IDisplayResult> DisplayAsync(TranslationsDeploymentStep step, BuildDisplayContext context)
    {
        return CombineAsync(
            View("TranslationsDeploymentStep_Fields_Summary", step).Location(OrchardCoreConstants.DisplayType.Summary, "Content"),
            View("TranslationsDeploymentStep_Fields_Thumbnail", step).Location("Thumbnail", "Content")
        );
    }

    public override async Task<IDisplayResult> EditAsync(TranslationsDeploymentStep step, BuildEditorContext context)
    {
        var supportedCultures = await _localizationService.GetSupportedCulturesAsync();
        var categories = await GetCategoriesAsync();

        return Initialize<TranslationsDeploymentStepViewModel>("TranslationsDeploymentStep_Fields_Edit", model =>
        {
            model.IncludeAll = step.IncludeAll;
            model.Cultures = step.Cultures;
            model.Categories = step.Categories;
            model.AllCultures = supportedCultures;
            model.AllCategories = categories;
        }).Location("Content");
    }

    public override async Task<IDisplayResult> UpdateAsync(TranslationsDeploymentStep step, UpdateEditorContext context)
    {
        step.Cultures = [];
        step.Categories = [];

        await context.Updater.TryUpdateModelAsync(step, Prefix, x => x.IncludeAll, x => x.Cultures, x => x.Categories);

        return await EditAsync(step, context);
    }

    private async Task<string[]> GetCategoriesAsync()
    {
        var categories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var provider in _localizationDataProviders)
        {
            var descriptors = await provider.GetDescriptorsAsync();
            foreach (var descriptor in descriptors)
            {
                categories.Add(descriptor.Context);
            }
        }

        return [.. categories.OrderBy(c => c)];
    }
}
