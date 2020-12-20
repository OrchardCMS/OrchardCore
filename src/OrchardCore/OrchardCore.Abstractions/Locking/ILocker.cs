using System;

namespace OrchardCore.Locking
{
    public interface ILocker : IDisposable, IAsyncDisposable
    {
    }
}
