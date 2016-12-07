using System;
using System.Linq;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Settings;
using Orchard.Tokens.Services;

namespace Orchard.Title.Handlers
{
    public class AutoroutePartHandler : ContentPartHandler<AutoroutePart>
    {
        private readonly ITokenizer _tokenizer;
        private readonly IAutorouteEntries _entries;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;

        public AutoroutePartHandler(
            IAutorouteEntries entries, 
            ITokenizer tokenizer, 
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService)
        {
            _tokenizer = tokenizer;
            _contentDefinitionManager = contentDefinitionManager;
            _entries = entries;
            _siteService = siteService;
        }

        public override void Published(PublishContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.AddEntry(part.ContentItem.ContentItemId, part.Path);
            }

            if (part.SetHomepage)
            {
                var site = _siteService.GetSiteSettingsAsync().Result;
                var homeRoute = site.HomeRoute;

                homeRoute["area"] = "Orchard.Contents";
                homeRoute["controller"] = "Item";
                homeRoute["action"] = "Display";
                homeRoute["id"] = context.ContentItem.ContentItemId;

                // Once we too the flag into account we can dismiss it.
                part.SetHomepage = false;
                _siteService.UpdateSiteSettingsAsync(site).Wait();
            }
        }

        public override void Unpublished(PublishContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.RemoveEntry(part.ContentItem.ContentItemId, part.Path);
            }
        }

        public override void Removed(RemoveContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.RemoveEntry(part.ContentItem.ContentItemId, part.Path);
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
            }
        }

        public override void GetContentItemAspect(ContentItemAspectContext context, AutoroutePart part)
        {
            context.For<ContentItemMetadata>(contentItemMetadata =>
            {
                contentItemMetadata.Identity.Add("Alias", part.Path);
            });
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
    }
}