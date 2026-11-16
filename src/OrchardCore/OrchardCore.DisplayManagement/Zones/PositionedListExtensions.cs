namespace OrchardCore.DisplayManagement.Zones;

internal static class PositionedListExtensions
{
    // Up to this count, the items are sorted in place with an insertion sort, which allocates nothing and is
    // the fastest for the few items a zone usually holds. Above it, a stable ordered copy is made.
    private const int InsertionSortThreshold = 16;

    /// <summary>
    /// Sorts the items by position, in place and keeping the order of the items with equal positions, as they
    /// were added. Nothing is allocated when the items are already in order, which is the common case.
    /// </summary>
    public static void SortByPosition(this List<IPositioned> items)
    {
        var count = items.Count;

        if (count < 2)
        {
            return;
        }

        var comparer = FlatPositionComparer.Instance;

        // Most items are added in their final order, so a single pass finds whether sorting is needed at all.
        var sorted = true;
        for (var i = 1; i < count; i++)
        {
            if (comparer.Compare(items[i - 1], items[i]) > 0)
            {
                sorted = false;
                break;
            }
        }

        if (sorted)
        {
            return;
        }

        if (count <= InsertionSortThreshold)
        {
            for (var i = 1; i < count; i++)
            {
                var item = items[i];
                var j = i - 1;

                // Strictly greater keeps equal positions in insertion order.
                while (j >= 0 && comparer.Compare(items[j], item) > 0)
                {
                    items[j + 1] = items[j];
                    j--;
                }

                items[j + 1] = item;
            }

            return;
        }

        // 'OrderBy' is stable, 'List.Sort' is not.
        var ordered = items.OrderBy(static x => x, comparer).ToArray();

        for (var i = 0; i < count; i++)
        {
            items[i] = ordered[i];
        }
    }
}
