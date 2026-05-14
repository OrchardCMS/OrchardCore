using OrchardCore.AdminMenu.Models;
using OrchardCore.Documents;

namespace OrchardCore.AdminMenu.Services;

internal sealed class AdminMenuAccessor : IAdminMenuAccessor
{
    private readonly IDocumentManager<AdminMenuList> _documentManager;

    public AdminMenuAccessor(IDocumentManager<AdminMenuList> documentManager) => _documentManager = documentManager;

    public async Task<IEnumerable<Models.AdminMenu>> GetAdminMenusAsync()
    {
        var adminMenuList = await _documentManager.GetOrCreateImmutableAsync();

        return adminMenuList.AdminMenu;
    }
}
