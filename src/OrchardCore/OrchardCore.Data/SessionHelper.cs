using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using YesSql;

namespace OrchardCore.Data
{
    /// <summary>
    /// Represents a class that provides helper methods for the <see cref="ISession"/>.
    /// </summary>
    public class SessionHelper : ISessionHelper
    {
        private readonly ISession _session;

        private readonly Dictionary<Type, object> _loaded = new Dictionary<Type, object>();

        /// <summary>
        /// Creates a new instance of <see cref="SessionHelper"/>.
        /// </summary>
        /// <param name="session">The <see cref="ISession"/>.</param>
        public SessionHelper(ISession session)
        {
            _session = session;
        }

        /// <inheritdocs/>
        public async Task<T> LoadForUpdateAsync<T>(Func<T> factory = null) where T : class, new()
        {
            if (_loaded.TryGetValue(typeof(T), out var loaded))
            {
                return loaded as T;
            }

            var document = await _session.Query<T>().FirstOrDefaultAsync() ?? factory?.Invoke() ?? new T();

            _loaded[typeof(T)] = document;

            return document;
        }

        /// <inheritdocs/>
        public async Task<T> GetForCachingAsync<T>(Func<T> factory = null) where T : class, new()
        {
            if (_loaded.TryGetValue(typeof(T), out var loaded))
            {
                _session.Detach(loaded);
            }

            var document = await _session.Query<T>().FirstOrDefaultAsync();

            if (document != null)
            {
                _session.Detach(document);
                return document;
            }

            return factory?.Invoke() ?? new T();
        }
    }
}
