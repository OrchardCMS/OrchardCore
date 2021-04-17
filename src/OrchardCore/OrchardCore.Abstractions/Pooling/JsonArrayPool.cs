using System;
using System.Buffers;
using Newtonsoft.Json;

namespace OrchardCore.Abstractions.Pooling
{
    internal sealed class JsonArrayPool<T> : IArrayPool<T>
    {
        private readonly ArrayPool<T> _inner;

        public JsonArrayPool(ArrayPool<T> inner)
        {
            if (inner == null)
            {
                ThrowArgumentNullException(nameof(inner));
            }

            _inner = inner;
        }

        public T[] Rent(int minimumLength)
        {
            return _inner.Rent(minimumLength);
        }

        public void Return(T[] array)
        {
            if (array == null)
            {
                ThrowArgumentNullException(nameof(array));
            }

            _inner.Return(array);
        }

        private static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}
