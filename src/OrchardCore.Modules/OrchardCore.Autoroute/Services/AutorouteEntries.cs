using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Documents;
using OrchardCore.Environment.Shell.Scope;
using YesSql;

namespace OrchardCore.Autoroute.Services
{
    public class AutorouteEntries : IAutorouteEntries
    {
        public AutorouteEntries()
        {
        }

        public async Task<string> TryGetContentItemIdAsync(string path)
        {
            var document = await GetDocumentAsync();
            document.ContentItemIds.TryGetValue(path.TrimEnd('/'), out var contentItemId);
            return contentItemId;
        }

        public async Task<string> TryGetPathAsync(string contentItemId)
        {
            var document = await GetDocumentAsync();
            document.Paths.TryGetValue(contentItemId, out var path);
            return path;
        }

        public async Task AddEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            var document = await LoadDocumentAsync();

            foreach (var entry in entries)
            {
                if (document.Paths.TryGetValue(entry.ContentItemId, out var previousPath))
                {
                    document.ContentItemIds.Remove(previousPath);
                }

                var requestPath = "/" + entry.Path.Trim('/');
                document.Paths[entry.ContentItemId] = requestPath;
                document.ContentItemIds[requestPath] = entry.ContentItemId;
            }

            await DocumentManager.UpdateAsync(document);
        }

        public async Task RemoveEntriesAsync(IEnumerable<AutorouteEntry> entries)
        {
            var document = await LoadDocumentAsync();

            foreach (var entry in entries)
            {
                document.Paths.Remove(entry.ContentItemId);
                document.ContentItemIds.Remove(entry.Path);
            }

            await DocumentManager.UpdateAsync(document);
        }

        /// <summary>
        /// Loads the autoroute document for updating and that should not be cached.
        /// </summary>
        private Task<AutorouteDocument> LoadDocumentAsync() => DocumentManager.GetMutableAsync();

        /// <summary>
        /// Gets the autoroute document for sharing and that should not be updated.
        /// </summary>
        private Task<AutorouteDocument> GetDocumentAsync()
        {
            return DocumentManager.GetImmutableAsync(() =>
            {
                var autoroutes = Session.QueryIndex<AutoroutePartIndex>(o => o.Published).ListAsync().GetAwaiter().GetResult();

                var document = new AutorouteDocument();

                foreach (var autoroute in autoroutes)
                {
                    var requestPath = "/" + autoroute.Path.Trim('/');
                    document.Paths[autoroute.ContentItemId] = requestPath;
                    document.ContentItemIds[requestPath] = autoroute.ContentItemId;
                }

                return document;
            });
        }

        private static ISession Session => ShellScope.Services.GetRequiredService<ISession>();

        private static IVolatileDocumentManager<AutorouteDocument> DocumentManager =>
            ShellScope.Services.GetRequiredService<IVolatileDocumentManager<AutorouteDocument>>();
    }
}
