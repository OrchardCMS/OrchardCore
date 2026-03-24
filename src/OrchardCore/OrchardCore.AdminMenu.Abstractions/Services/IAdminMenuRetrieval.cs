namespace OrchardCore.AdminMenu.Services;

public interface IAdminMenuRetrieval
{
    Task<IEnumerable<Models.AdminMenu>> GetAdminMenusAsync();
}
