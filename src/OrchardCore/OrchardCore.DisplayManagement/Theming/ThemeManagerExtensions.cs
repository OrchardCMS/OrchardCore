using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Extensions;

namespace OrchardCore.DisplayManagement.Theming;

public static class ThemeManagerExtensions
{
    /// <summary>
    /// Asynchronously retrieves the shape table for the current theme.
    /// </summary>
    /// <param name="themeManager">The theme manager used to retrieve the current theme.</param>
    /// <param name="shapeTableManager">The shape table manager used to retrieve the shape table for the theme.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ShapeTable"/>
    /// associated with the current theme.</returns>
    public static Task<ShapeTable> GetShapeTableAsync(this IThemeManager themeManager, IShapeTableManager shapeTableManager)
    {
        var themeTask = themeManager.GetThemeAsync();

        if (themeTask.IsCompletedSuccessfully)
        {
            var theme = themeTask.Result;
            return shapeTableManager.GetShapeTableAsync(theme?.Id);
        }

        return GetShapeTableAwaitedAsync(themeTask, shapeTableManager);

        async static Task<ShapeTable> GetShapeTableAwaitedAsync(Task<IExtensionInfo> themeTask, IShapeTableManager shapeTableManager)
        {
            var theme = await themeTask;
            return await shapeTableManager.GetShapeTableAsync(theme?.Id);
        }
    }

    /// <summary>
    /// Asynchronously retrieves the shape table for the current theme.
    /// </summary>
    /// <param name="themeManager">The theme manager used to retrieve the current theme.</param>
    /// <param name="shapeTableManager">The shape table manager used to retrieve the shape table for the theme.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="ShapeTable"/>
    /// associated with the current theme. If there is no active theme, the result will be null.
    /// </returns>
    public static Task<ShapeTable> TryGetShapeTableAsync(this IThemeManager themeManager, IShapeTableManager shapeTableManager)
    {
        var themeTask = themeManager.GetThemeAsync();

        if (themeTask.IsCompletedSuccessfully)
        {
            var theme = themeTask.Result;

            if(theme == null)
            {
                return Task.FromResult<ShapeTable>(null);
            }

            return shapeTableManager.GetShapeTableAsync(theme?.Id);
        }

        return GetShapeTableAwaitedAsync(themeTask, shapeTableManager);

        async static Task<ShapeTable> GetShapeTableAwaitedAsync(Task<IExtensionInfo> themeTask, IShapeTableManager shapeTableManager)
        {
            var theme = await themeTask;

            if (theme == null)
            {
                return null;
            }

            return await shapeTableManager.GetShapeTableAsync(theme?.Id);
        }
    }
}
