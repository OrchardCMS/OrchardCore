using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public abstract class AdminNodeDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IAdminMenuRetrieval _adminMenuRetrieval;

    public AdminNodeDataLocalizationProvider(IAdminMenuRetrieval adminMenuRetrieval)
    {
        _adminMenuRetrieval = adminMenuRetrieval;
    }

    public abstract Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync();

    protected async Task<IEnumerable<Models.AdminMenu>> GetAdminMenuAsync()
        => await _adminMenuRetrieval.GetAdminMenusAsync();
}
