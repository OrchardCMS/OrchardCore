using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using OrchardCore.Environment.Cache;
using OrchardCore.Sitemaps.Models;
using YesSql;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapService : ISitemapService
    {
        private const string SitemapDocumentTokenKey = "SitemapDocumentTokenKey";

        private readonly ISignal _signal;
        private readonly SitemapEntries _sitemapEntries;
        private readonly ISession _session;

        public SitemapService(
            ISignal signal,
            SitemapEntries sitemapEntries,
            ISession session
            )
        {
            _signal = signal;
            _session = session;
            _sitemapEntries = sitemapEntries;
        }

        public IChangeToken ChangeToken => _signal.GetToken(SitemapDocumentTokenKey);

        public void SaveSitemapDocument(SitemapDocument document)
        {
            // Always clear and rebuild the sitemap route entries on save.
            BuildAllSitemapRouteEntries(document);

            _session.Save(document);
            _signal.SignalToken(SitemapDocumentTokenKey);
        }

        public void BuildAllSitemapRouteEntries(SitemapDocument document)
        {
            var entries = new List<SitemapEntry>();
            foreach (var sitemapSet in document.SitemapSets.Where(x => x.Enabled))
            {
                entries.AddRange(GetSitemapRouteEntries(sitemapSet.SitemapNodes));
            }
            _sitemapEntries.BuildEntries(entries);
        }

        public async Task<SitemapDocument> LoadSitemapDocumentAsync()
        {
            SitemapDocument sitemapDocument;

            sitemapDocument = await _session.Query<SitemapDocument>().FirstOrDefaultAsync();

            if (sitemapDocument == null)
            {
                lock (this)
                {
                    sitemapDocument = new SitemapDocument();
                    _session.Save(sitemapDocument);
                    _signal.SignalToken(SitemapDocumentTokenKey);
                }
            }
            else
            {
                sitemapDocument.SetNodeSet();
            }

            return sitemapDocument;
        }
        private List<SitemapEntry> GetSitemapRouteEntries(IList<SitemapNode> sitemapNodes, List<SitemapEntry> sitemapEntries = null)
        {
            if (sitemapEntries == null)
            {
                sitemapEntries = new List<SitemapEntry>();
            }

            foreach (var sitemapNode in sitemapNodes)
            {
                sitemapEntries.Add(new SitemapEntry { Path = sitemapNode.Path, SitemapNodeId = sitemapNode.Id });
                if (sitemapNode.ChildNodes != null)
                {
                    // Recurse with child nodes.
                    GetSitemapRouteEntries(sitemapNode.ChildNodes, sitemapEntries);
                }
            }

            return sitemapEntries;
        }
    }
}
