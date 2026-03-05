namespace OrchardCore.DisplayManagement.Descriptors;

public static class PlacementInfoExtensions
{
    /// <summary>
    /// Combines two <see cref="PlacementInfo"/>.
    /// </summary>
    /// <remarks>
    /// Second overrides first. First and second can be null.
    /// </remarks>
    /// <param name="first">First placement.</param>
    /// <param name="second">Second placement.</param>
    /// <returns>Combined <see cref="PlacementInfo"/>.</returns>
    public static PlacementInfo Combine(this PlacementInfo first, PlacementInfo second)
    {
        if (first == null)
        {
            return second;
        }
        else if (second != null)
        {
            var combined = new PlacementInfo(
                string.IsNullOrEmpty(second.Location) ? first.Location : second.Location,
                $"{first.Source},{second.Source}",
                string.IsNullOrEmpty(second.ShapeType) ? first.ShapeType : second.ShapeType,
                string.IsNullOrEmpty(second.DefaultPosition) ? first.DefaultPosition : second.DefaultPosition,
                CombineArrays(first.Alternates, second.Alternates),
                CombineArrays(first.Wrappers, second.Wrappers)
            );

            return combined;
        }

        return first;
    }

    /// <summary>
    /// Combines the alternates from the specified placement with the provided alternates array.
    /// </summary>
    /// <param name="placement">The placement information whose alternates will be combined. Can be null.</param>
    /// <param name="alternates">An array of alternate strings to combine with the placement's alternates. Can be null.</param>
    /// <returns>An array containing all alternates from both the placement and the provided array. Will be null if both sources are null,
    /// or empty if one or both sources are empty.
    /// </returns>
    public static string[] CombineAlternates(this PlacementInfo placement, string[] alternates)
        => CombineArrays(placement?.Alternates, alternates);

    /// <summary>
    /// Combines the wrapper arrays from the specified placement and the provided wrappers into a single array.
    /// </summary>
    /// <param name="placement">The placement information whose wrapper array will be combined. Can be null.</param>
    /// <param name="wrappers">An array of wrapper strings to combine with the placement's wrappers. Can be null.</param>
    /// <returns>An array containing all wrappers from both the placement and the provided wrappers. Will be null if both sources are null,
    /// or empty if one or both sources are empty.
    /// </returns>
    public static string[] CombineWrappers(this PlacementInfo placement, string[] wrappers)
        => CombineArrays(placement?.Wrappers, wrappers);

    /// <summary>
    /// Combines two string arrays.
    /// </summary>
    /// <remarks>
    /// First and second can be null. Returns null if both are null.
    /// </remarks>
    /// <param name="first">First array.</param>
    /// <param name="second">Second array.</param>
    /// <returns>Combined array or null.</returns>
    private static string[] CombineArrays(string[] first, string[] second)
    {
        if (first == null || first.Length == 0)
        {
            return second ?? first;
        }
        else if (second != null && second.Length > 0)
        {
            var combined = new string[first.Length + second.Length];
            Array.Copy(first, 0, combined, 0, first.Length);
            Array.Copy(second, 0, combined, first.Length, second.Length);
            return combined;
        }

        return first ?? second;
    }
}
