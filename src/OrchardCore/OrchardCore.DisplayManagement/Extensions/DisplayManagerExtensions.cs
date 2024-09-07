using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.DisplayManagement;

public static class DisplayManagerExtensions
{
    public static Task<IShape> BuildDisplayAsync<TModel>(this IDisplayManager<TModel> displayManager, IUpdateModel updater, string displayType = "", string groupId = "") where TModel : new()
        => displayManager.BuildDisplayAsync(new TModel(), updater, displayType, groupId);

    public static Task<IShape> BuildEditorAsync<TModel>(this IDisplayManager<TModel> displayManager, IUpdateModel updater, bool isNew, string groupId = "", string htmlPrefix = "") where TModel : new()
        => displayManager.BuildEditorAsync(new TModel(), updater, isNew, groupId, htmlPrefix);
}
