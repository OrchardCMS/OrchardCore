using OrchardCore.DataSources;

namespace OrchardCore.Tests.Modules.OrchardCore.DataSources;

internal static class DataTestHelpers
{
    public static async IAsyncEnumerable<DataBatch> ToBatches(IReadOnlyList<DataField> fields, params object[][] rows)
    {
        await Task.Yield();

        yield return new DataBatch(fields, rows);
    }

    public static async Task<List<DataBatch>> ToListAsync(IAsyncEnumerable<DataBatch> batches)
    {
        var list = new List<DataBatch>();

        await foreach (var batch in batches)
        {
            list.Add(batch);
        }

        return list;
    }

    public static async Task<(IReadOnlyList<DataField> Fields, List<object[]> Rows)> ReadAllAsync(IAsyncEnumerable<DataBatch> batches)
    {
        IReadOnlyList<DataField> fields = [];
        var rows = new List<object[]>();

        await foreach (var batch in batches)
        {
            fields = batch.Fields;
            rows.AddRange(batch.Rows);
        }

        return (fields, rows);
    }
}
