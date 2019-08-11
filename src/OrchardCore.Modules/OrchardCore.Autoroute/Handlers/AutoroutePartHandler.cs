using System;
using System.Linq;
using System.Threading.Tasks;
using Fluid;
using OrchardCore.Autoroute.Model;
using OrchardCore.Autoroute.Models;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Records;
using OrchardCore.ContentManagement.Routing;
using OrchardCore.Environment.Cache;
using OrchardCore.Liquid;
using OrchardCore.Settings;
using YesSql;

namespace OrchardCore.Autoroute.Handlers
{
    public class AutoroutePartHandler : ContentPartHandler<AutoroutePart>
    {
        private readonly IAutorouteEntries _entries;
        private readonly ILiquidTemplateManager _liquidTemplateManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ITagCache _tagCache;
        private readonly YesSql.ISession _session;

        public AutoroutePartHandler(
            IAutorouteEntries entries,
            ILiquidTemplateManager liquidTemplateManager,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            ITagCache tagCache,
            YesSql.ISession session)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _entries = entries;
            _liquidTemplateManager = liquidTemplateManager;
            _siteService = siteService;
            _tagCache = tagCache;
            _session = session;
        }

        public override async Task PublishedAsync(PublishContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.AddEntry(part.ContentItem.ContentItemId, part.Path);
            }

            if (part.SetHomepage)
            {
                var site = await _siteService.GetSiteSettingsAsync();
                var homeRoute = site.HomeRoute;

                homeRoute["area"] = "OrchardCore.Contents";
                homeRoute["controller"] = "Item";
                homeRoute["action"] = "Display";
                homeRoute["contentItemId"] = context.ContentItem.ContentItemId;

                // Once we too the flag into account we can dismiss it.
                part.SetHomepage = false;
                await _siteService.UpdateSiteSettingsAsync(site);
            }

            // Evict any dependent item from cache
            await RemoveTagAsync(part);
        }

        public override Task UnpublishedAsync(PublishContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.RemoveEntry(part.ContentItem.ContentItemId, part.Path);

                // Evict any dependent item from cache
                return RemoveTagAsync(part);
            }

            return Task.CompletedTask;
        }

        public override Task RemovedAsync(RemoveContentContext context, AutoroutePart part)
        {
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                _entries.RemoveEntry(part.ContentItem.ContentItemId, part.Path);

                // Evict any dependent item from cache
                return RemoveTagAsync(part);
            }

            return Task.CompletedTask;
        }

        public override async Task UpdatedAsync(UpdateContentContext context, AutoroutePart part)
        {
            // Compute the Path only if it's empty
            if (!String.IsNullOrWhiteSpace(part.Path))
            {
                return;
            }

            var pattern = GetPattern(part);

            if (!String.IsNullOrEmpty(pattern))
            {
                var templateContext = new TemplateContext();
                templateContext.SetValue("ContentItem", part.ContentItem);

                part.Path = await _liquidTemplateManager.RenderAsync(pattern, NullEncoder.Default, templateContext);
                part.Path = part.Path.Replace("\r", String.Empty).Replace("\n", String.Empty);

                if (!await IsPathUniqueAsync(part.Path, part))
                {
                    part.Path = await GenerateUniquePathAsync(part.Path, part);
                }

                part.Apply();
            }
        }

        public async override Task CloningAsync(CloneContentContext context, AutoroutePart part)
        {
            var clonedPart = context.CloneContentItem.As<AutoroutePart>();
            clonedPart.Path = await GenerateUniquePathAsync(part.Path, clonedPart);
            clonedPart.SetHomepage = false;
            clonedPart.Apply();
        }

        private Task RemoveTagAsync(AutoroutePart part)
        {
            return _tagCache.RemoveTagAsync($"alias:{part.Path}");
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

        private async Task<string> GenerateUniquePathAsync(string path, AutoroutePart context)
        {
            var version = 1;
            var unversionedPath = path;

            var versionSeparatorPosition = path.LastIndexOf('-');
            if (versionSeparatorPosition > -1 && int.TryParse(path.Substring(versionSeparatorPosition).TrimStart('-'), out version))
            {
                unversionedPath = path.Substring(0, versionSeparatorPosition);
            }

            while (true)
            {
                var versionedPath = $"{unversionedPath}-{version++}";
                if (await IsPathUniqueAsync(versionedPath, context))
                {
                    return versionedPath;
                }
            }
        }

        private async Task<bool> IsPathUniqueAsync(string path, AutoroutePart context)
        {
            return (await _session.QueryIndex<AutoroutePartIndex>(o => o.ContentItemId != context.ContentItem.ContentItemId && o.Path == path).CountAsync()) == 0;
        }
    }
}
