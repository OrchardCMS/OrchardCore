using OrchardCore.Localization.Data;

namespace OrchardCore.AdminMenu.Services;

public abstract class AdminNodeDataLocalizationProvider : ILocalizationDataProvider
{
    private readonly IAdminMenuAccessor _adminMenuRetrieval;

    public AdminNodeDataLocalizationProvider(IAdminMenuAccessor adminMenuRetrieval)
    {
        _adminMenuRetrieval = adminMenuRetrieval;
    }

    public abstract Task<IEnumerable<DataLocalizedString>> GetDescriptorsAsync();

    protected async Task<IEnumerable<Models.AdminMenu>> GetAdminMenusAsync()
        => await _adminMenuRetrieval.GetAdminMenusAsync();
}
