using System.Runtime.CompilerServices;

namespace OrchardCore.DataSources.Operations;

/// <summary>
/// Adapts rows read synchronously, such as from a temporary file, to the asynchronous sequences operations read.
/// </summary>
internal static class DataAsyncRows
{
    public static async IAsyncEnumerable<object[]> From(IEnumerable<object[]> rows, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var count = 0;

        foreach (var row in rows)
        {
            if (++count % 4096 == 0)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Yield();
            }

            yield return row;
        }
    }
}
