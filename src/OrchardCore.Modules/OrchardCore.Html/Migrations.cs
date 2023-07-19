using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.ContentManagement.Records;
using OrchardCore.Data.Migration;
using OrchardCore.Html.Settings;
using YesSql;

namespace OrchardCore.Html
{
    public class Migrations : DataMigration
    {
        private readonly ISession _session;
        private readonly ILogger _logger;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(
            IContentDefinitionManager contentDefinitionManager,
            ISession session,
            ILogger<Migrations> logger)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _session = session;
            _logger = logger;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("HtmlBodyPart", builder => builder
                .Attachable()
                .WithDescription("Provides an HTML Body for your content item."));

            // Shortcut other migration steps on new content definition schemas.
            return 5;
        }

        // This code can be removed in a later version.
#pragma warning disable CA1822 // Mark members as static
        public int UpdateFrom1()
        {
            return 2;
        }

        // This code can be removed in a later version.
        public int UpdateFrom2()
#pragma warning restore CA1822 // Mark members as static
        {
            return 3;
        }

        // This code can be removed in a later version.
        public async Task<int> UpdateFrom3()
        {
            // Update content type definitions
            foreach (var contentType in _contentDefinitionManager.LoadTypeDefinitions())
            {
                if (contentType.Parts.Any(x => x.PartDefinition.Name == "BodyPart"))
                {
                    _contentDefinitionManager.AlterTypeDefinition(contentType.Name, x => x.RemovePart("BodyPart").WithPart("HtmlBodyPart"));
                }
            }

            _contentDefinitionManager.DeletePartDefinition("BodyPart");

            // We are patching all content item versions by moving the Title to DisplayText
            // This step doesn't need to be executed for a brand new site

            var lastDocumentId = 0L;

            for (; ; )
            {
                var contentItemVersions = await _session.Query<ContentItem, ContentItemIndex>(x => x.DocumentId > lastDocumentId).Take(10).ListAsync();

                if (!contentItemVersions.Any())
                {
                    // No more content item version to process
                    break;
                }

                foreach (var contentItemVersion in contentItemVersions)
                {
                    if (UpdateBody(contentItemVersion.Content))
                    {
                        _session.Save(contentItemVersion);
                        _logger.LogInformation("A content item version's BodyPart was upgraded: {ContentItemVersionId}", contentItemVersion.ContentItemVersionId);
                    }

                    lastDocumentId = contentItemVersion.Id;
                }

                await _session.SaveChangesAsync();
            }

            static bool UpdateBody(JToken content)
            {
                var changed = false;

                if (content.Type == JTokenType.Object)
                {
                    var body = content["BodyPart"]?["Body"]?.Value<string>();

                    if (!String.IsNullOrWhiteSpace(body))
                    {
                        content["HtmlBodyPart"] = new JObject(new JProperty("Html", body));
                        changed = true;
                    }
                }

                foreach (var token in content)
                {
                    changed = UpdateBody(token) || changed;
                }

                return changed;
            }

            return 4;
        }

        // This code can be removed in a later version.
        public int UpdateFrom4()
        {
            // For backwards compatability with liquid filters we disable html sanitization on existing field definitions.
            foreach (var contentType in _contentDefinitionManager.LoadTypeDefinitions())
            {
                if (contentType.Parts.Any(x => x.PartDefinition.Name == "HtmlBodyPart"))
                {
                    _contentDefinitionManager.AlterTypeDefinition(contentType.Name, x => x.WithPart("HtmlBodyPart", part =>
                    {
                        part.MergeSettings<HtmlBodyPartSettings>(x => x.SanitizeHtml = false);
                    }));
                }
            }

            return 5;
        }
    }
}
