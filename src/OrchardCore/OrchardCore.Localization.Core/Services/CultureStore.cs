using System;
using System.Linq;
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

        public async Task<CultureRecord> GetCultureRecordAsync()
        {
            var cultureRecord = await _session.Query<CultureRecord>().FirstOrDefaultAsync();
            if (cultureRecord == null)
            {
                cultureRecord = new CultureRecord();
                _session.Save(cultureRecord);
            }

            return cultureRecord;
        }

        public Task SaveAsync(string culture, CancellationToken cancellationToken)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var cultureRecord = GetCultureRecordAsync().Result;

            if (!cultureRecord.Cultures.Any(c => c.CultureName == culture))
            {
                cultureRecord.Cultures.Add(new Culture { CultureName = culture });
                _session.Save(cultureRecord);
                return _session.CommitAsync();
            }
            return null;
        }

        public Task DeleteAsync(string culture, CancellationToken cancellationToken)
        {
            if (culture == null)
            {
                throw new ArgumentNullException(nameof(culture));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var cultureRecord = GetCultureRecordAsync().Result;
            var recordToDelete = cultureRecord.Cultures.Where(c => c.CultureName == culture).FirstOrDefault();

            if (recordToDelete != null)
            {
                cultureRecord.Cultures.Remove(recordToDelete);
                _session.Save(cultureRecord);
                return _session.CommitAsync();
            }

            return null;
        }
    }
}
