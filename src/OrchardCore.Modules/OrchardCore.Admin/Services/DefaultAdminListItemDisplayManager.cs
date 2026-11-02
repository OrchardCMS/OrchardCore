using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Admin.Services;

public sealed class DefaultAdminListItemDisplayManager<TModel> : IAdminListItemDisplayManager<TModel>
{
    private readonly IDisplayManager<TModel> _displayManager;

    public DefaultAdminListItemDisplayManager(IDisplayManager<TModel> displayManager)
    {
        _displayManager = displayManager;
    }

    public Task<IShape> BuildDisplayAsync(TModel model, IUpdateModel updater, string displayType)
        => _displayManager.BuildDisplayAsync(model, updater, displayType);
}
