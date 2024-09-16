using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Data.Documents;

/// <summary>
/// A singleton service using the file system to store document files under the tenant folder, and that is in sync
/// with the ambient transaction, any file is updated after a successful <see cref="IDocumentStore.CommitAsync"/>.
/// </summary>
public class FileDocumentStore : IFileDocumentStore
{
    private readonly string _tenantPath;

    private readonly SemaphoreSlim _semaphore = new(1);

    public FileDocumentStore(IOptions<ShellOptions> shellOptions, ShellSettings shellSettings)
    {
        _tenantPath = Path.Combine(
            shellOptions.Value.ShellsApplicationDataPath,
            shellOptions.Value.ShellsContainerName,
            shellSettings.Name) + "/";

        Directory.CreateDirectory(_tenantPath);
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateMutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new()
    {
        var loaded = ShellScope.Get<T>(typeof(T));
        if (loaded != null)
        {
            return loaded;
        }

        var document = await GetDocumentAsync<T>()
            ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null))
            ?? new T();

        ShellScope.Set(typeof(T), document);

        return document;
    }

    /// <inheritdoc />
    public async Task<(bool, T)> GetOrCreateImmutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new()
    {
        var loaded = ShellScope.Get<T>(typeof(T));
        if (loaded != null)
        {
            // Return the already loaded document but indicating that it should not be cached.
            return (false, loaded);
        }

        return (true, await GetDocumentAsync<T>() ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null)) ?? new T());
    }

    /// <inheritdoc />
    public Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false)
    {
        DocumentStore.AfterCommitSuccess<T>(async () =>
        {
            await SaveDocumentAsync(document);
            ShellScope.Set(typeof(T), null);
            await updateCache(document);
        });

        return Task.CompletedTask;
    }

    public Task CancelAsync() => DocumentStore.CancelAsync();
    public void AfterCommitSuccess<T>(DocumentStoreCommitSuccessDelegate afterCommitSuccess) => DocumentStore.AfterCommitSuccess<T>(afterCommitSuccess);
    public void AfterCommitFailure<T>(DocumentStoreCommitFailureDelegate afterCommitFailure) => DocumentStore.AfterCommitFailure<T>(afterCommitFailure);
    public Task CommitAsync() => throw new NotImplementedException();

    private async Task<T> GetDocumentAsync<T>()
    {
        var typeName = typeof(T).Name;

        var attribute = typeof(T).GetCustomAttribute<FileDocumentStoreAttribute>();
        if (attribute != null)
        {
            typeName = attribute.FileName ?? typeName;
        }

        var filename = _tenantPath + typeName + ".json";
        if (!File.Exists(filename))
        {
            return default;
        }

        await _semaphore.WaitAsync();
        try
        {
            using var stream = File.OpenRead(filename);
            return await JsonSerializer.DeserializeAsync<T>(stream, JOptions.Default);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task SaveDocumentAsync<T>(T document)
    {
        var typeName = typeof(T).Name;

        var attribute = typeof(T).GetCustomAttribute<FileDocumentStoreAttribute>();
        if (attribute != null)
        {
            typeName = attribute.FileName ?? typeName;
        }

        var filename = _tenantPath + typeName + ".json";

        await _semaphore.WaitAsync();
        try
        {
            using var stream = File.Create(filename);
            await JsonSerializer.SerializeAsync(stream, document, JOptions.Indented);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private static IDocumentStore DocumentStore => ShellScope.Services.GetRequiredService<IDocumentStore>();
}
