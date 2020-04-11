using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL.DataLoader;

namespace OrchardCore.Apis.GraphQL
{
    public static class DataLoaderExtensions
    {
        public static Task<T[]> LoadAsync<TKey, T>(this IDataLoader<TKey, T> dataLoader, IEnumerable<TKey> keys)
        {
            var tasks = new List<Task<T>>();

            foreach (var key in keys)
            {
                tasks.Add(dataLoader.LoadAsync(key));
            }

            return Task.WhenAll(tasks);
        }

        public static Task<T[]> LoadAsync<TKey, T>(this IDataLoader<TKey, T> dataLoader, params TKey[] keys)
        {
            return dataLoader.LoadAsync(keys.AsEnumerable());
        }
    }
}
