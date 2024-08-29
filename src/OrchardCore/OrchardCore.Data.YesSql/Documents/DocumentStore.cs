using YesSql;

namespace OrchardCore.Data.Documents;

/// <summary>
/// An <see cref="IDocumentStore"/> using the <see cref="ISession"/>.
/// </summary>
public class DocumentStore : IDocumentStore
{
    private readonly ISession _session;

    private readonly Dictionary<Type, object> _loaded = [];

    private readonly List<Type> _afterCommitsSuccess = [];
    private readonly List<Type> _afterCommitsFailure = [];

    private DocumentStoreCommitSuccessDelegate _afterCommitSuccess;
    private DocumentStoreCommitFailureDelegate _afterCommitFailure;

    private bool _canceled;

    public DocumentStore(ISession session)
    {
        _session = session;
    }

    /// <inheritdoc />
    public async Task<T> GetOrCreateMutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new()
    {
        if (_loaded.TryGetValue(typeof(T), out var loaded))
        {
            return loaded as T;
        }

        var document = await _session.Query<T>().FirstOrDefaultAsync()
            ?? await (factoryAsync?.Invoke() ?? Task.FromResult((T)null))
            ?? new T();

        _loaded[typeof(T)] = document;

        return document;
    }

    /// <inheritdoc />
    public async Task<(bool, T)> GetOrCreateImmutableAsync<T>(Func<Task<T>> factoryAsync = null) where T : class, new()
    {
        if (_loaded.TryGetValue(typeof(T), out var loaded))
        {
            // Return the already loaded document but indicating that it should not be cached.
            return (false, loaded as T);
        }

        var document = await _session.Query<T>().FirstOrDefaultAsync();
        if (document is not null)
        {
            _session.Detach(document);
            return (true, document);
        }

        return (true, await (factoryAsync?.Invoke() ?? Task.FromResult((T)null)) ?? new T());
    }

    /// <inheritdoc />
    public async Task UpdateAsync<T>(T document, Func<T, Task> updateCache, bool checkConcurrency = false)
    {
        await _session.SaveAsync(document, checkConcurrency);

        AfterCommitSuccess<T>(() =>
        {
            return updateCache(document);
        });

        AfterCommitFailure<T>(exception =>
        {
            throw new DocumentStoreCommitException(
                $"The '{typeof(T).Name}' could not be persisted and cached as it has been changed by another process.",
                exception);
        });
    }

    /// <inheritdoc />
    public async Task CancelAsync()
    {
        _canceled = true;
        await _session.CancelAsync();
    }

    /// <inheritdoc />
    public void AfterCommitSuccess<T>(DocumentStoreCommitSuccessDelegate afterCommitSuccess)
    {
        if (!_afterCommitsSuccess.Contains(typeof(T)))
        {
            _afterCommitsSuccess.Add(typeof(T));
            _afterCommitSuccess += afterCommitSuccess;
        }
    }

    /// <inheritdoc />
    public void AfterCommitFailure<T>(DocumentStoreCommitFailureDelegate afterCommitFailure)
    {
        if (!_afterCommitsFailure.Contains(typeof(T)))
        {
            _afterCommitsFailure.Add(typeof(T));
            _afterCommitFailure += afterCommitFailure;
        }
    }

    /// <inheritdoc />
    public async Task CommitAsync()
    {
        if (_session is null)
        {
            return;
        }

        try
        {
            await _session.SaveChangesAsync();

            _loaded.Clear();

            if (!_canceled && _afterCommitSuccess is not null)
            {
                foreach (var d in _afterCommitSuccess.GetInvocationList())
                {
                    await ((DocumentStoreCommitSuccessDelegate)d)();
                }
            }
        }
        catch (ConcurrencyException exception)
        {
            if (_afterCommitFailure is not null)
            {
                foreach (var d in _afterCommitFailure.GetInvocationList())
                {
                    await ((DocumentStoreCommitFailureDelegate)d)(exception);
                }
            }
            else
            {
                throw;
            }
        }
    }
}
