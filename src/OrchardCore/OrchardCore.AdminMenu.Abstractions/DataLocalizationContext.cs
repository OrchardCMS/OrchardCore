namespace OrchardCore.AdminMenu;

public static class DataLocalizationContext
{
    public static string AdminMenu(string menuName = null) => menuName is null
        ? "Admin Menus"
        : $"Admin Menus{OrchardCoreConstants.DataLocalizationSeparator}{menuName}";
}
