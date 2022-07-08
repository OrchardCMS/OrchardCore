using System.Buffers;
using Newtonsoft.Json;

namespace OrchardCore.Abstractions.Pooling
{
    /// <summary>
    /// An adapter for JSON.NET to allow usage of ArrayPool.
    /// </summary>
    internal sealed class JsonArrayPool<T> : IArrayPool<T>
    {
        private readonly ArrayPool<T> _inner;

        public JsonArrayPool(ArrayPool<T> inner) => _inner = inner;

        public T[] Rent(int minimumLength) => _inner.Rent(minimumLength);

        public void Return(T[] array) => _inner.Return(array);
    }
}
