namespace OrchardCore.Layers;

/// <summary>
/// The admin list of layers rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="LayersAdminListColumnProvider"/>.
/// </summary>
public static class LayersAdminList
{
    /// <summary>
    /// The name of the list, used by <see cref="Admin.IAdminListColumnProvider"/> and the
    /// <c>AdminList__Layers</c> and <c>AdminListCell__Layers__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Layers";
}
