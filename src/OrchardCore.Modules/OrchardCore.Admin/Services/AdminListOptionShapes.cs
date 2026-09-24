using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Admin.Services;

/// <summary>
/// Finds the layouts declared by option shapes, e.g. <c>AdminListLayout-Grid.Option.cshtml</c> declares the
/// <c>AdminListLayout_Option__Grid</c> shape, so the <c>Grid</c> layout.
/// </summary>
internal static class AdminListOptionShapes
{
    /// <summary>
    /// Gets the names the option shapes starting with the given prefix declare, e.g. <c>grid</c> for
    /// <see cref="AdminListConstants.OptionShapePrefix"/>, in alphabetical order. The names of template shapes
    /// are lowercase, as the shape table harvests them.
    /// </summary>
    public static IEnumerable<string> GetNames(ShapeTable shapeTable, string prefix)
        => shapeTable.Bindings
            .Where(binding => binding.Key.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            // A shape described in code keeps the name it was described with.
            .Select(binding => (binding.Value?.BindingName ?? binding.Key)[prefix.Length..])
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase);
}
