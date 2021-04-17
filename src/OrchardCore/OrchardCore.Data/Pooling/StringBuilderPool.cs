using System;
using System.Diagnostics;
using System.Text;

namespace OrchardCore.Data.Pooling
{
    /// <summary>
    /// The usage is:
    ///        var inst = PooledStringBuilder.GetInstance();
    ///        var sb = inst.builder;
    ///        ... Do Stuff...
    ///        ... sb.ToString() ...
    ///        inst.Free();
    /// </summary>
    internal sealed class StringBuilderPool : IDisposable
    {
        private const int DefaultCapacity = 1 * 1024;

        // global pool
        private static readonly ObjectPool<StringBuilderPool> s_poolInstance = CreatePool();

        public readonly StringBuilder Builder = new StringBuilder(DefaultCapacity);
        private readonly ObjectPool<StringBuilderPool> _pool;

        private StringBuilderPool(ObjectPool<StringBuilderPool> pool)
        {
            Debug.Assert(pool != null);
            _pool = pool;
        }

        public int Length => Builder.Length;

        internal static ObjectPool<StringBuilderPool> CreatePool(int size = 1000)
        {
            ObjectPool<StringBuilderPool> pool = null;
            pool = new ObjectPool<StringBuilderPool>(() => new StringBuilderPool(pool), size);
            return pool;
        }

        public static StringBuilderPool GetInstance()
        {
            var builder = s_poolInstance.Allocate();
            Debug.Assert(builder.Builder.Length == 0);
            return builder;
        }

        public override string ToString()
        {
            return Builder.ToString();
        }

        public void Dispose()
        {
            var builder = Builder;

            // Do not store builders that are too large.

            if (builder.Capacity == DefaultCapacity)
            {
                builder.Clear();
                _pool.Free(this);
            }
        }
    }
}
