using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Documents;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Options;

/// <summary>
/// Defers distributed options invalidation until the current shell scope has committed successfully.
/// </summary>
public sealed class DefaultOptionsUpdateNotifier : IOptionsUpdateNotifier
{
    private readonly IDocumentStore _documentStore;
    private readonly HashSet<OptionsUpdateRequest> _pendingUpdates = [];

    private bool _commitRegistered;
    private bool _commitSucceeded;
    private bool _deferredTaskAdded;

    public DefaultOptionsUpdateNotifier(IDocumentStore documentStore) => _documentStore = documentStore;

    public IOptionsUpdateNotifier RequestUpdate<TOptions>(string name = "") where TOptions : class
    {
        if (ShellScope.Current is null)
        {
            throw new InvalidOperationException($"'{nameof(IOptionsUpdateNotifier)}' can only be used from an active shell scope.");
        }

        _pendingUpdates.Add(new(typeof(TOptions), name ?? Microsoft.Extensions.Options.Options.DefaultName));

        if (!_commitRegistered)
        {
            _commitRegistered = true;

            _documentStore.AfterCommitSuccess<DefaultOptionsUpdateNotifier>(() =>
            {
                _commitSucceeded = true;
                return Task.CompletedTask;
            });
        }

        if (_deferredTaskAdded)
        {
            return this;
        }

        _deferredTaskAdded = true;

        ShellScope.AddDeferredTask(async scope =>
        {
            try
            {
                if (!_commitSucceeded || _pendingUpdates.Count == 0)
                {
                    return;
                }

                var signal = scope.ServiceProvider.GetRequiredService<ISignal>();

                foreach (var update in _pendingUpdates)
                {
                    await signal.SignalTokenAsync(OptionsUpdateSignal.GetKey(update.OptionsType, update.Name));
                }
            }
            finally
            {
                _pendingUpdates.Clear();
                _commitRegistered = false;
                _commitSucceeded = false;
                _deferredTaskAdded = false;
            }
        });

        return this;
    }

    private readonly record struct OptionsUpdateRequest(Type OptionsType, string Name);
}
