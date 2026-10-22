namespace OrchardCore.Admin;

/// <summary>
/// Describes an admin list whose source items are rendered using the display manager for <typeparamref name="TModel"/>.
/// </summary>
/// <typeparam name="TModel">The display model type, including a base class or interface for derived items.</typeparam>
public sealed class AdminListContext<TModel> : AdminListContext
{
    /// <summary>
    /// Creates a list description with its item type set to <typeparamref name="TModel"/>.
    /// </summary>
    public AdminListContext()
    {
        ItemType = typeof(TModel);
    }

    /// <summary>
    /// The typed model items to render after the list selects its layout.
    /// </summary>
    public new IEnumerable<TModel> Source
    {
        get => (IEnumerable<TModel>)base.Source;
        set => base.Source = value;
    }
}
