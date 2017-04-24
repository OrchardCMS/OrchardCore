using System;
using System.Linq;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.ContentManagement.Records;
using Orchard.Environment.Cache;
using Orchard.Settings;
using Orchard.Tokens.Services;
using YesSql;

namespace Orchard.Autoroute.Handlers
{
    public class AutoroutePartHandler : ContentPartHandler<AutoroutePart>
    {
        private readonly ITokenizer _tokenizer;
        private readonly IAutorouteEntries _entries;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ITagCache _tagCache;
        private readonly YesSql.ISession _session;

        public AutoroutePartHandler(
            IAutorouteEntries entries,
            ITokenizer tokenizer,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            ITagCache tagCache,
            YesSql.ISession session)
        {
            _tokenizer = tokenizer;
            _contentDefinitionManager = contentDefinitionManager;
            _entries = entries;
            _siteService = siteService;
            _tagCache = tagCache;
            _session = session;
        }

        public override void Published(PublishContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.AddEntry(part.ContentItem.ContentItemId, part.Path);
            }

            if (part.SetHomepage)
            {
                var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
                var homeRoute = site.HomeRoute;

                homeRoute["area"] = "Orchard.Contents";
                homeRoute["controller"] = "Item";
                homeRoute["action"] = "Display";
                homeRoute["contentItemId"] = context.ContentItem.ContentItemId;

                // Once we too the flag into account we can dismiss it.
                part.SetHomepage = false;
                _siteService.UpdateSiteSettingsAsync(site).GetAwaiter().GetResult();
            }

            // Evict any dependent item from cache
            RemoveTag(part);
        }

        public override void Unpublished(PublishContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.RemoveEntry(part.ContentItem.ContentItemId, part.Path);

                // Evict any dependent item from cache
                RemoveTag(part);
            }
        }

        public override void Removed(RemoveContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.RemoveEntry(part.ContentItem.ContentItemId, part.Path);

                // Evict any dependent item from cache
                RemoveTag(part);
            }
        }

        public override void Updated(UpdateContentContext context, AutoroutePart part)
        {
            // Compute the Path only if it's empty
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                return;
            }

            var pattern = GetPattern(part);

            if (!String.IsNullOrEmpty(pattern))
            {
                var ctx = _tokenizer
                    .CreateViewModel()
                    .Content(part.ContentItem);

                part.Path = _tokenizer.Tokenize(pattern, ctx);
                if (!IsPathUnique(part.Path, part))
                {
                    part.Path = GenerateUniquePath(part.Path, part);
                }

                part.Apply();
            }
        }

        private void RemoveTag(AutoroutePart part)
        {
            _tagCache.RemoveTag($"alias:{part.Path}");
        }

        /// <summary>
        /// Get the pattern from the AutoroutePartSettings property for its type
        /// </summary>
        private string GetPattern(AutoroutePart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, "AutoroutePart", StringComparison.Ordinal));
            var pattern = contentTypePartDefinition.Settings.ToObject<AutoroutePartSettings>().Pattern;

            return pattern;
        }

        private string GenerateUniquePath(string path, AutoroutePart context)
        {
            var version = 1;
            var unversionedPath = path;

            var versionSeparatorPosition = path.LastIndexOf('-');
            if (versionSeparatorPosition > -1)
            {
                int.TryParse(path.Substring(versionSeparatorPosition).TrimStart('-'), out version);
                unversionedPath = path.Substring(0, versionSeparatorPosition);
            }

            while (true)
            {
                var versionedPath = $"{unversionedPath}-{version++}";
                if (IsPathUnique(versionedPath, context))
                {
                    return versionedPath;
                }
            }
        }

        private bool IsPathUnique(string path, AutoroutePart context)
        {
            return _session.QueryIndexAsync<AutoroutePartIndex>(o => o.ContentItemId != context.ContentItem.ContentItemId && o.Path == path).Count().GetAwaiter().GetResult() == 0;
        }
    }
}