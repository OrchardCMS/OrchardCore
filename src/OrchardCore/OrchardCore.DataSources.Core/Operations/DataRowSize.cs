namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Estimates the memory rows take, to decide when an operation writes them to temporary files.
/// </summary>
internal static class DataRowSize
{
    // The header of an object or an array, and a reference, on a 64-bit runtime.
    private const int ObjectHeader = 24;
    private const int Reference = 8;

    public static long Estimate(object[] row)
    {
        if (row is null)
        {
            return 0;
        }

        long size = ObjectHeader + (Reference * row.Length);

        foreach (var value in row)
        {
            size += Estimate(value);
        }

        return size;
    }

    public static long Estimate(object value)
    {
        return value switch
        {
            null => 0,
            string text => ObjectHeader + (2L * text.Length),
            decimal or Guid or DateTimeOffset => ObjectHeader + 16,
            DataPartialAggregate part => ObjectHeader + 8 + (3 * Reference) + Estimate(part.Sum) + Estimate(part.Min) + Estimate(part.Max),
            _ => ObjectHeader + 8,
        };
    }
}
