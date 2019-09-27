using System;
using System.Collections.Generic;

namespace OrchardCore.Sitemaps.Services
{
    public class SitemapEntries
    {
        private readonly Dictionary<string, string> _paths;
        private readonly Dictionary<string, string> _sitemapNodeIds;

        public SitemapEntries()
        {
            _paths = new Dictionary<string, string>();
            _sitemapNodeIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public bool TryGetSitemapNodeId(string path, out string sitemapNodeId)
        {
            path = '/' + path;
            return _sitemapNodeIds.TryGetValue(path, out sitemapNodeId);
        }

        public void AddEntry(SitemapEntry entry)
        {
            lock (this)
            {
                if (_paths.TryGetValue(entry.SitemapNodeId, out var previousPath))
                {
                    _sitemapNodeIds.Remove(previousPath);
                }

                var requestPath = "/" + entry.Path.TrimStart('/');
                _paths[entry.SitemapNodeId] = requestPath;
                _sitemapNodeIds[requestPath] = entry.SitemapNodeId;
            }
        }

        public void AddEntries(IEnumerable<SitemapEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    if (_paths.TryGetValue(entry.SitemapNodeId, out var previousPath))
                    {
                        _sitemapNodeIds.Remove(previousPath);
                    }

                    var requestPath = "/" + entry.Path.TrimStart('/');
                    _paths[entry.SitemapNodeId] = requestPath;
                    _sitemapNodeIds[requestPath] = entry.SitemapNodeId;
                }
            }
        }

        public void RemoveEntries(IEnumerable<SitemapEntry> entries)
        {
            lock (this)
            {
                foreach (var entry in entries)
                {
                    _paths.Remove(entry.SitemapNodeId);
                    _sitemapNodeIds.Remove(entry.Path);
                }
            }
        }

        public void RemoveEntry(SitemapEntry entry)
        {
            lock (this)
            {
                _paths.Remove(entry.SitemapNodeId);
                _sitemapNodeIds.Remove(entry.Path);
            }
        }
    }
}
