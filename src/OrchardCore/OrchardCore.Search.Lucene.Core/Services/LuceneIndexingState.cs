using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Search.Lucene;

/// <summary>
/// This class persists the indexing state, a cursor, on the filesystem alongside the index itself.
/// This state has to be on the filesystem as each node has its own local storage for the index.
/// </summary>
public sealed class LuceneIndexingState : ILuceneIndexingState
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly string _stateFileName;

    private JsonObject _stateDocument;

    public LuceneIndexingState(
        IOptions<ShellOptions> shellOptions,
        ShellSettings shellSettings
        )
    {
        _stateFileName = PathExtensions.Combine(
            shellOptions.Value.ShellsApplicationDataPath,
            shellOptions.Value.ShellsContainerName,
            shellSettings.Name,
            "lucene.status.json");
    }

    public async Task<long> GetLastTaskIdAsync(string indexFullName)
    {
        ArgumentNullException.ThrowIfNull(indexFullName);

        await EnsureContentIsSetAsync();

        if (_stateDocument.TryGetPropertyValue(indexFullName, out var value))
        {
            return value.Value<long>();
        }

        await SetLastTaskIdAsync(indexFullName, 0);

        return 0;
    }

    public async Task SetLastTaskIdAsync(string indexFullName, long taskId)
    {
        ArgumentNullException.ThrowIfNull(indexFullName);

        await EnsureContentIsSetAsync();

        await _semaphore.WaitAsync();

        _stateDocument[indexFullName] = taskId > 0 ? taskId : 0;

        try
        {
            await File.WriteAllTextAsync(_stateFileName, _stateDocument.ToJsonString(JOptions.Indented));
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task EnsureContentIsSetAsync()
    {
        if (_stateDocument is not null)
        {
            return;
        }

        await _semaphore.WaitAsync();

        try
        {
            if (_stateDocument is not null)
            {
                return;
            }

            if (!File.Exists(_stateFileName))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_stateFileName));

                await File.WriteAllTextAsync(_stateFileName, new JsonObject().ToJsonString(JOptions.Indented));
            }

            _stateDocument = JObject.Parse(await File.ReadAllTextAsync(_stateFileName));
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
