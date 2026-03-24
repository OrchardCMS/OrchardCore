using OrchardCore.AdminMenu.Models;
using OrchardCore.Documents;

namespace OrchardCore.AdminMenu.Services;

public class AdminMenuRetrieval : IAdminMenuRetrieval
{
    private readonly IDocumentManager<AdminMenuList> _documentManager;

    public AdminMenuRetrieval(IDocumentManager<AdminMenuList> documentManager) => _documentManager = documentManager;

    public async Task<IEnumerable<Models.AdminMenu>> GetAdminMenusAsync()
    {
        var adminMenuList = await _documentManager.GetOrCreateImmutableAsync();

        return adminMenuList.AdminMenu;
    }
}
