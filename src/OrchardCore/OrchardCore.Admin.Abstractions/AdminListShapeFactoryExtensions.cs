using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin;

public static class AdminListShapeFactoryExtensions
{
    /// <summary>
    /// Creates the <c>AdminListActions</c> shape rendering the <c>Actions</c> and <c>ActionsMenu</c> zones of a row
    /// in the configured actions layout, e.g. <c>@await DisplayAsync(await Factory.CreateAdminListActionsAsync((IShape)Model))</c>
    /// in a row template. It is created with its properties rather than through the dynamic <c>New</c>, since it is
    /// rendered once per row.
    /// </summary>
    /// <param name="factory">The <see cref="IShapeFactory"/>.</param>
    /// <param name="row">The row shape, e.g. a <c>SummaryAdmin</c> shape.</param>
    /// <param name="listName">The name of the list the row belongs to. When empty, it is read from the row, which the list stamps it on.</param>
    public static async ValueTask<IShape> CreateAdminListActionsAsync(this IShapeFactory factory, IShape row, string listName = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(row);

        var actions = await factory.CreateAsync(AdminListActionsLayouts.ShapeType);

        actions.Properties["Row"] = row;

        if (!string.IsNullOrEmpty(listName))
        {
            actions.Properties[AdminListConstants.ListNameProperty] = listName;
        }

        return actions;
    }
}
