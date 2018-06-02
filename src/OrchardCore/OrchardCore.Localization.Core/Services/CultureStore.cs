using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using OrchardCore.Localization.Models;
using YesSql;

namespace OrchardCore.Localization.Services
{
    public class CultureStore : ICultureStore
    {
        private readonly ISession _session;

        public CultureStore(ISession session)
        {
            _session = session;
        }

        public void Dispose()
        {
        }

        public Task<IEnumerable<ICulture>> GetAllCultures() {
            return _session.Query<ICulture>().ListAsync();
        }

        public Task SaveAsync(ICulture culture, CancellationToken cancellationToken)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(culture);
            return _session.CommitAsync();
        }

        public Task DeleteAsync(ICulture culture, CancellationToken cancellationToken)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Delete(culture);
            return _session.CommitAsync();
        }

        public Task<ICulture> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.GetAsync<ICulture>(int.Parse(identifier, CultureInfo.InvariantCulture));
        }
    }
}
