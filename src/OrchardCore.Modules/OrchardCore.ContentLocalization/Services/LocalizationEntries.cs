using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentLocalization.Models;
using OrchardCore.ContentLocalization.Records;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.ContentLocalization.Services
{
    public class LocalizationEntries : ILocalizationEntries
    {
        public LocalizationEntries()
        {
        }

        public async Task<(bool, LocalizationEntry)> TryGetLocalizationAsync(string contentItemId)
        {
            var document = await GetDocumentAsync();

            if (document.Localizations.TryGetValue(contentItemId, out var localization))
            {
                return (true, localization);
            }

            return (false, localization);
        }

        public async Task<IEnumerable<LocalizationEntry>> GetLocalizationsAsync(string localizationSet)
        {
            var document = await GetDocumentAsync();

            if (document.LocalizationSets.TryGetValue(localizationSet, out var localizations))
            {
                return localizations;
            }

            return Enumerable.Empty<LocalizationEntry>();
        }

        public async Task AddEntriesAsync(IEnumerable<LocalizationEntry> entries)
        {
            var document = await LoadDocumentAsync();
            AddEntries(document, entries);
            await DocumentManager.UpdateAsync(document);
        }

        public async Task RemoveEntriesAsync(IEnumerable<LocalizationEntry> entries)
        {
            var document = await LoadDocumentAsync();
            RemoveEntries(document, entries);
            await DocumentManager.UpdateAsync(document);
        }

        private void AddEntries(LocalizationDocument document, IEnumerable<LocalizationEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (document.Localizations.ContainsKey(entry.ContentItemId))
                {
                    continue;
                }

                document.Localizations[entry.ContentItemId] = entry;

                if (document.LocalizationSets.TryGetValue(entry.LocalizationSet, out var localizations))
                {
                    localizations.Add(entry);
                }
                else
                {
                    localizations = new List<LocalizationEntry>();
                    document.LocalizationSets[entry.LocalizationSet] = localizations;
                    localizations.Add(entry);
                }
            }
        }

        public void RemoveEntries(LocalizationDocument document, IEnumerable<LocalizationEntry> entries)
        {
            foreach (var entry in entries)
            {
                if (!document.Localizations.ContainsKey(entry.ContentItemId))
                {
                    continue;
                }

                document.Localizations.Remove(entry.ContentItemId);

                if (document.LocalizationSets.TryGetValue(entry.LocalizationSet, out var localizations))
                {
                    localizations.RemoveAll(l => l.Culture == entry.Culture);
                }
            }
        }

        /// <summary>
        /// Loads the localization document for updating and that should not be cached.
        /// </summary>
        private Task<LocalizationDocument> LoadDocumentAsync() => DocumentManager.GetOrCreateMutableAsync(CreateDocumentAsync);

        /// <summary>
        /// Gets the localization document for sharing and that should not be updated.
        /// </summary>
        private Task<LocalizationDocument> GetDocumentAsync() => DocumentManager.GetOrCreateImmutableAsync(CreateDocumentAsync);

        private async Task<LocalizationDocument> CreateDocumentAsync()
        {
            var indexes = await Session.QueryIndex<LocalizedContentItemIndex>(i => i.Published).ListAsync();

            var document = new LocalizationDocument();

            AddEntries(document, indexes.Select(i => new LocalizationEntry
            {
                ContentItemId = i.ContentItemId,
                LocalizationSet = i.LocalizationSet,
                Culture = i.Culture.ToLowerInvariant()
            }));

            return document;
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IVolatileDocumentManager<LocalizationDocument> DocumentManager
            => ShellScope.Services.GetRequiredService<IVolatileDocumentManager<LocalizationDocument>>();
    }
}
