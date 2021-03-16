using System;
using System.Threading.Tasks;

namespace OrchardCore.Locking
{
    public static class LockerExtensions
    {
        public static ValueTask DisposeAsync(this ILocker locker)
        {
            if (locker is IAsyncDisposable asyncDisposable)
            {
                return asyncDisposable.DisposeAsync();
            }

            locker.Dispose();

            return default;
        }
    }
}
