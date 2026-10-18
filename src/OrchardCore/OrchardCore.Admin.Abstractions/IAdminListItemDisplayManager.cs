using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Admin;

/// <summary>
/// Adapts a model's display manager for admin list rows. The default adapter uses IDisplayManager&lt;TModel&gt;.
/// Register a closed implementation for models with a specialized display manager, such as content items.
/// </summary>
/// <typeparam name="TModel">The row model type.</typeparam>
public interface IAdminListItemDisplayManager<TModel>
{
    /// <summary>
    /// Builds a row shape using the selected admin list display type.
    /// </summary>
    /// <param name="model">The row model.</param>
    /// <param name="updater">The current update model.</param>
    /// <param name="displayType">The selected display type.</param>
    Task<IShape> BuildDisplayAsync(TModel model, IUpdateModel updater, string displayType);
}
