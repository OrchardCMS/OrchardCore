using System;
using System.Linq;
using Orchard.Autoroute.Model;
using Orchard.Autoroute.Models;
using Orchard.Autoroute.Services;
using Orchard.ContentManagement.Handlers;
using Orchard.ContentManagement.MetaData;
using Orchard.Tokens.Services;

namespace Orchard.Title.Handlers
{
    public class AutoroutePartHandler : ContentPartHandler<AutoroutePart>
    {
        private readonly ITokenizer _tokenizer;
        private readonly IAutorouteEntries _entries;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AutoroutePartHandler(IAutorouteEntries entries, ITokenizer tokenizer, IContentDefinitionManager contentDefinitionManager)
        {
            _tokenizer = tokenizer;
            _contentDefinitionManager = contentDefinitionManager;
            _entries = entries;
        }

        public override void Published(PublishContentContext context, AutoroutePart instance)
        {
            if (!String.IsNullOrWhiteSpace(instance.Path))
            {
                _entries.AddEntry(instance.ContentItem.ContentItemId, instance.Path);
            }
        }

        public override void Unpublished(PublishContentContext context, AutoroutePart instance)
        {
            if (!String.IsNullOrWhiteSpace(instance.Path))
            {
                _entries.RemoveEntry(instance.ContentItem.ContentItemId, instance.Path);
            }
        }

        public override void Removed(RemoveContentContext context, AutoroutePart instance)
        {
            if (!String.IsNullOrWhiteSpace(instance.Path))
            {
                _entries.RemoveEntry(instance.ContentItem.ContentItemId, instance.Path);
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

        public override void GetContentItemMetadata(ContentItemMetadataContext context, AutoroutePart part)
        {
            context.Metadata.Identity.Add("Alias", part.Path);
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