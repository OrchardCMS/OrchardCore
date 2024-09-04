namespace OrchardCore.Environment.Shell;

public static class ShellStringExtensions
{
    /// <summary>
    /// Whether or not the provided name is the 'Default' tenant name.
    /// </summary>
    public static bool IsDefaultShellName(this string name) => name == ShellSettings.DefaultShellName;

    /// <summary>
    /// Whether or not the provided name may be in conflict with the 'Default' tenant name.
    /// </summary>
    public static bool IsDefaultShellNameIgnoreCase(this string name) =>
        name is not null && name.Equals(ShellSettings.DefaultShellName, StringComparison.OrdinalIgnoreCase);
}
