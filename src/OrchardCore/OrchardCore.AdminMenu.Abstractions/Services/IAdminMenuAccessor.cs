namespace OrchardCore.AdminMenu.Services;

public interface IAdminMenuAccessor
{
    Task<IEnumerable<Models.AdminMenu>> GetAdminMenusAsync();
}
