using System;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.BackgroundTasks.Services
{
    /// <summary>
    /// An empty change token that is always in the changed state but without raising any change callbacks.
    /// </summary>
    internal class AlwaysHasChangedToken : IChangeToken
    {
        public static AlwaysHasChangedToken Singleton { get; } = new AlwaysHasChangedToken();

        private AlwaysHasChangedToken()
        {
        }

        public bool HasChanged => true;

        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object> callback, object state)
        {
            return EmptyDisposable.Instance;
        }
    }

    internal class EmptyDisposable : IDisposable
    {
        public static EmptyDisposable Instance { get; } = new EmptyDisposable();

        private EmptyDisposable()
        {
        }

        public void Dispose()
        {
        }
    }
}
