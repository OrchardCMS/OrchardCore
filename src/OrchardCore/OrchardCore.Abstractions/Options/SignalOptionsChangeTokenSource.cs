using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;

namespace OrchardCore.Environment.Options;

/// <summary>
/// Provides an Orchard Core distributed signal-backed <see cref="IOptionsChangeTokenSource{TOptions}"/>
/// so the default <see cref="IOptionsMonitor{TOptions}"/> can refresh options after an update notification.
/// </summary>
/// <remarks>
/// The change token is triggered either by the key of this options type and name, or by
/// <see cref="OptionsUpdateSignal.AllKey"/> that invalidates every signal-backed options instance.
/// </remarks>
/// <typeparam name="TOptions">The options type to observe.</typeparam>
public sealed class SignalOptionsChangeTokenSource<TOptions> : IOptionsChangeTokenSource<TOptions>
    where TOptions : class
{
    private readonly ISignal _signal;
    private readonly string _key;

    /// <summary>
    /// Creates a change-token source for the default options name.
    /// </summary>
    /// <param name="signal">The Orchard Core signal service, or <see langword="null"/> if there is none, in that case the options never change.</param>
    public SignalOptionsChangeTokenSource(ISignal signal)
        : this(signal, Microsoft.Extensions.Options.Options.DefaultName)
    {
    }

    /// <summary>
    /// Creates a change-token source for the specified named options instance.
    /// </summary>
    /// <param name="signal">The Orchard Core signal service, or <see langword="null"/> if there is none, in that case the options never change.</param>
    /// <param name="name">The named options instance to observe.</param>
    public SignalOptionsChangeTokenSource(ISignal signal, string name)
    {
        _signal = signal;
        Name = name ?? Microsoft.Extensions.Options.Options.DefaultName;
        _key = OptionsUpdateSignal.GetKey(typeof(TOptions), Name);
    }

    /// <summary>
    /// Gets the named options instance observed by this token source.
    /// </summary>
    public string Name { get; }

    /// <inheritdoc />
    public IChangeToken GetChangeToken()
    {
        // Without signal, e.g. in an application that doesn't register the caching services, nothing can update the options.
        if (_signal is null)
        {
            return NullChangeToken.Singleton;
        }

        return new SignalChangeToken(_signal.GetToken(_key), _signal.GetToken(OptionsUpdateSignal.AllKey));
    }

    /// <summary>
    /// A change token triggered by either of two signal tokens.
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="CompositeChangeToken"/>, the callbacks don't capture the execution context in which they
    /// are registered, so the options that the monitor eagerly rebuilds use the shell scope of the signaling flow,
    /// not the one, cleared since, in which the monitor registered its callbacks.
    /// </remarks>
    private sealed class SignalChangeToken : IChangeToken
    {
        private readonly IChangeToken _optionsToken;
        private readonly IChangeToken _allToken;

        public SignalChangeToken(IChangeToken optionsToken, IChangeToken allToken)
        {
            _optionsToken = optionsToken;
            _allToken = allToken;
        }

        public bool HasChanged
            => _optionsToken.HasChanged || _allToken.HasChanged;

        public bool ActiveChangeCallbacks
            => true;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            var registration = new Registration(callback, state);

            registration.Add(_optionsToken.RegisterChangeCallback(Registration.Invoke, registration));
            registration.Add(_allToken.RegisterChangeCallback(Registration.Invoke, registration));

            return registration;
        }

        private sealed class Registration : IDisposable
        {
            private readonly Action<object> _callback;
            private readonly object _state;
            private readonly List<IDisposable> _registrations = [];

            private int _invoked;

            public Registration(Action<object> callback, object state)
            {
                _callback = callback;
                _state = state;
            }

            public static void Invoke(object state)
            {
                var registration = (Registration)state;

                // Both tokens may be triggered, but the callback is only invoked once.
                if (Interlocked.Exchange(ref registration._invoked, 1) == 0)
                {
                    registration._callback(registration._state);
                }
            }

            public void Add(IDisposable registration)
            {
                lock (_registrations)
                {
                    _registrations.Add(registration);
                }
            }

            public void Dispose()
            {
                lock (_registrations)
                {
                    foreach (var registration in _registrations)
                    {
                        registration.Dispose();
                    }

                    _registrations.Clear();
                }
            }
        }
    }
}
