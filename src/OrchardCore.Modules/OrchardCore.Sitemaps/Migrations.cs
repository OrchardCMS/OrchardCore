using System;
using System.Collections.Generic;
using System.Text;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.Data.Migration;
using OrchardCore.ContentManagement.Metadata.Settings;

namespace OrchardCore.Sitemaps
{
    public class Migrations : DataMigration
    {
        IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("SitemapPart", builder => builder
                .Attachable()
                .WithDescription("Provides a part that allows sitemap settings on a content item."));

            return 1;
        }

    }
}
