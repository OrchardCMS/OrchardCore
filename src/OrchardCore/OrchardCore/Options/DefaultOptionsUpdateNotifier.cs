using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Documents;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Distributed;
using OrchardCore.Environment.Shell.Scope;

namespace OrchardCore.Environment.Options;

/// <summary>
/// Defers distributed options invalidation until the current shell scope has committed successfully.
/// </summary>
/// <remarks>
/// The options are invalidated in the order they were first requested, so an options type that
/// is built from another one should be requested after it. All the options of the dependent tenants,
/// see <see cref="ShellContext.AddDependentShellAsync(ShellContext)"/>, are then invalidated.
/// </remarks>
public sealed class DefaultOptionsUpdateNotifier : IOptionsUpdateNotifier
{
    private readonly IDocumentStore _documentStore;
    private readonly List<OptionsUpdateRequest> _pendingUpdates = [];
    private readonly HashSet<OptionsUpdateRequest> _requestedUpdates = [];

    private bool _commitRegistered;
    private bool _commitSucceeded;
    private bool _deferredTaskAdded;

    public DefaultOptionsUpdateNotifier(IDocumentStore documentStore)
    {
        _documentStore = documentStore;
    }

    public IOptionsUpdateNotifier RequestUpdate<TOptions>(string name = "") where TOptions : class
    {
        if (ShellScope.Current is null)
        {
            throw new InvalidOperationException($"'{nameof(IOptionsUpdateNotifier)}' can only be used from an active shell scope.");
        }

        var request = new OptionsUpdateRequest(typeof(TOptions), name ?? Microsoft.Extensions.Options.Options.DefaultName);

        if (_requestedUpdates.Add(request))
        {
            _pendingUpdates.Add(request);
        }

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

                // Let the other instances of this tenant know that its options have been updated.
                var shellOptionsEvents = scope.ServiceProvider.GetService<ShellOptionsEvents>();
                if (shellOptionsEvents is not null)
                {
                    await shellOptionsEvents.InvokeUpdatedAsync(scope.ShellContext.Settings.Name);
                }

                // The tenants that depend on this tenant, e.g. that validate the tokens issued by it, may build their
                // options from its state, so all their options are also invalidated, here and on the other instances.
                foreach (var dependent in await scope.ShellContext.GetDependentShellsAsync())
                {
                    var dependentScope = await dependent.CreateScopeAsync();
                    if (dependentScope is null)
                    {
                        continue;
                    }

                    await dependentScope.UsingAsync(dependentScope =>
                        OptionsUpdateSignal.SignalAllAsync(dependentScope.ServiceProvider.GetRequiredService<ISignal>()));

                    if (shellOptionsEvents is not null)
                    {
                        await shellOptionsEvents.InvokeUpdatedAsync(dependent.Settings.Name);
                    }
                }
            }
            finally
            {
                _pendingUpdates.Clear();
                _requestedUpdates.Clear();
                _commitRegistered = false;
                _commitSucceeded = false;
                _deferredTaskAdded = false;
            }
        });

        return this;
    }

    private readonly record struct OptionsUpdateRequest(Type OptionsType, string Name);
}
