using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using Fluid.Values;
using Microsoft.Extensions.Localization;
using OrchardCore.Alias.Models;
using OrchardCore.Alias.Settings;
using OrchardCore.Alias.ViewModels;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;
using YesSql;

namespace OrchardCore.Alias.Handlers
{
    public class AliasPartHandler : ContentPartHandler<AliasPart>
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ITagCache _tagCache;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly ISession _session;
        protected readonly IStringLocalizer S;

        public AliasPartHandler(
            IContentDefinitionManager contentDefinitionManager,
            ITagCache tagCache,
            ILiquidTemplateManager liquidTemplateManager,
            ISession session,
            IStringLocalizer<AliasPartHandler> stringLocalizer)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _tagCache = tagCache;
            _liquidTemplateManager = liquidTemplateManager;
            _session = session;
            S = stringLocalizer;
        }

        public override async Task ValidatingAsync(ValidateContentContext context, AliasPart part)
        {
            // Only validate the alias if it's not empty.
            if (String.IsNullOrWhiteSpace(part.Alias))
            {
                return;
            }

            await foreach (var item in part.ValidateAsync(S, _session))
            {
                context.Fail(item);
            }
        }

        public async override Task UpdatedAsync(UpdateContentContext context, AliasPart part)
        {
            // Compute the Alias only if it's empty.
            if (!String.IsNullOrEmpty(part.Alias))
            {
                return;
            }

            var pattern = GetPattern(part);

            if (!String.IsNullOrEmpty(pattern))
            {
                var model = new AliasPartViewModel()
                {
                    Alias = part.Alias,
                    AliasPart = part,
                    ContentItem = part.ContentItem
                };

                part.Alias = await _liquidTemplateManager.RenderStringAsync(pattern, NullEncoder.Default, model,
                    new Dictionary<string, FluidValue>() { [nameof(ContentItem)] = new ObjectValue(model.ContentItem) });

                part.Alias = part.Alias.Replace("\r", String.Empty).Replace("\n", String.Empty);

                if (part.Alias?.Length > AliasPart.MaxAliasLength)
                {
                    part.Alias = part.Alias[..AliasPart.MaxAliasLength];
                }

                if (!await part.IsAliasUniqueAsync(_session, part.Alias))
                {
                    part.Alias = await GenerateUniqueAliasAsync(part.Alias, part);
                }

                part.Apply();
            }
        }

        public override Task PublishedAsync(PublishContentContext context, AliasPart instance)
        {
            return _tagCache.RemoveTagAsync($"alias:{instance.Alias}");
        }

        public override Task RemovedAsync(RemoveContentContext context, AliasPart instance)
        {
            if (context.NoActiveVersionLeft)
            {
                return _tagCache.RemoveTagAsync($"alias:{instance.Alias}");
            }

            return Task.CompletedTask;
        }

        public override Task UnpublishedAsync(PublishContentContext context, AliasPart instance)
        {
            return _tagCache.RemoveTagAsync($"alias:{instance.Alias}");
        }

        public override async Task CloningAsync(CloneContentContext context, AliasPart part)
        {
            var clonedPart = context.CloneContentItem.As<AliasPart>();
            clonedPart.Alias = await GenerateUniqueAliasAsync(part.Alias, clonedPart);

            clonedPart.Apply();
        }

        /// <summary>
        /// Get the pattern from the AliasPartSettings property for its type.
        /// </summary>
        private string GetPattern(AliasPart part)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(part.ContentItem.ContentType);
            var contentTypePartDefinition = contentTypeDefinition.Parts.FirstOrDefault(x => String.Equals(x.PartDefinition.Name, nameof(AliasPart)));
            var pattern = contentTypePartDefinition.GetSettings<AliasPartSettings>().Pattern;

            return pattern;
        }

        private async Task<string> GenerateUniqueAliasAsync(string alias, AliasPart context)
        {
            var version = 1;
            var unversionedAlias = alias;

            var versionSeparatorPosition = alias.LastIndexOf('-');
            if (versionSeparatorPosition > -1 && Int32.TryParse(alias[versionSeparatorPosition..].TrimStart('-'), out version))
            {
                unversionedAlias = alias[..versionSeparatorPosition];
            }

            while (true)
            {
                // Unversioned length + separator char + version length.
                var quantityCharactersToTrim = unversionedAlias.Length + 1 + version.ToString().Length - AliasPart.MaxAliasLength;
                if (quantityCharactersToTrim > 0)
                {
                    unversionedAlias = unversionedAlias[..^quantityCharactersToTrim];
                }

                var versionedAlias = $"{unversionedAlias}-{version++}";
                if (await context.IsAliasUniqueAsync(_session, versionedAlias))
                {
                    return versionedAlias;
                }
            }
        }
    }
}
