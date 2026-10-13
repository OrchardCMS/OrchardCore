namespace OrchardCore.Layers;

/// <summary>
/// The admin list of layers rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="LayersAdminListColumnDriver"/>.
/// </summary>
public static class LayersAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Layers</c> and <c>AdminListCell__Layers__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Layers";
}
