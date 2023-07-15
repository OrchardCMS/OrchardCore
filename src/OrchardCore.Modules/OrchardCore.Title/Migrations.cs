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
using YesSql;

namespace OrchardCore.Title
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISession _session;
        private readonly ILogger _logger;

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
            _contentDefinitionManager.AlterPartDefinition("TitlePart", builder => builder
                .Attachable()
                .WithDescription("Provides a Title for your content item.")
                .WithDefaultPosition("0")
                );

            // Shortcut other migration steps on new content definition schemas.
            return 2;
        }

        // This code can be removed in a later version.
        public async Task<int> UpdateFrom1()
        {
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
                    if (String.IsNullOrEmpty(contentItemVersion.DisplayText)
                        && UpdateTitle(contentItemVersion.Content))
                    {
                        _session.Save(contentItemVersion);
                        _logger.LogInformation("A content item version's Title was upgraded: {ContentItemVersionId}", contentItemVersion.ContentItemVersionId);
                    }

                    lastDocumentId = contentItemVersion.Id;
                }

                await _session.SaveChangesAsync();
            }

            static bool UpdateTitle(JToken content)
            {
                var changed = false;

                if (content.Type == JTokenType.Object)
                {
                    var title = content["TitlePart"]?["Title"]?.Value<string>();

                    if (!String.IsNullOrWhiteSpace(title))
                    {
                        content["DisplayText"] = title;
                        changed = true;
                    }
                }

                foreach (var token in content)
                {
                    changed = UpdateTitle(token) || changed;
                }

                return changed;
            }

            return 2;
        }
    }
}
