using Dapper;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using YesSql;

namespace OrchardCore.Html
{
    public class Migrations : DataMigration
    {
        private const string RENAME_BODYPARTS_SQL = @"
--UPDATES Module definitions
UPDATE Document SET Content = REPLACE(content, 'OrchardCore.Body', 'OrchardCore.Html') 
WHERE Type = 'OrchardCore.Environment.Shell.Descriptor.Models.ShellDescriptor, OrchardCore.Environment.Shell.Abstractions';

UPDATE Document SET Content = REPLACE(content, 'OrchardCore.Body', 'OrchardCore.Html') 
WHERE Type = 'OrchardCore.Environment.Shell.State.ShellState, OrchardCore.Environment.Shell.Abstractions';


--UPDATES migrations
UPDATE Document SET Content = REPLACE(content, 'OrchardCore.Body.Migrations', 'OrchardCore.Html.Migrations') 
WHERE Type = 'OrchardCore.Data.Migration.Records.DataMigrationRecord, OrchardCore.Data';


--UPDATES Content Type definitions
UPDATE Document SET Content = REPLACE(content, 'BodyPart', 'HtmlBodyPart') 
WHERE Type = 'OrchardCore.ContentManagement.Metadata.Records.ContentDefinitionRecord, OrchardCore.ContentManagement.Abstractions';

UPDATE Document SET Content = REPLACE(content, 'MarkdownPart', 'MarkdownBodyPart') 
WHERE Type = 'OrchardCore.ContentManagement.Metadata.Records.ContentDefinitionRecord, OrchardCore.ContentManagement.Abstractions';


--UPDATES content items
UPDATE Document SET Content = REPLACE(content, 'BodyPart', 'HtmlBodyPart') 
WHERE Type = 'OrchardCore.ContentManagement.ContentItem, OrchardCore.ContentManagement.Abstractions';

UPDATE Document SET Content = REPLACE(content, 'MarkdownPart', 'MarkdownBodyPart') 
WHERE Type = 'OrchardCore.ContentManagement.ContentItem, OrchardCore.ContentManagement.Abstractions';

UPDATE Document SET Content = REPLACE(content, '{""Body""', '{""Html""') 
WHERE Type = 'OrchardCore.ContentManagement.ContentItem, OrchardCore.ContentManagement.Abstractions';
";

        IContentDefinitionManager _contentDefinitionManager;
        private readonly IStore _store;

        public Migrations(IContentDefinitionManager contentDefinitionManager, IStore store)
        {
            _contentDefinitionManager = contentDefinitionManager;
            _store = store;
        }

        public int Create()
        {
            _contentDefinitionManager.AlterPartDefinition("HtmlBodyPart", builder => builder
                .Attachable()
                .WithDescription("Provides an HTML Body for your content item."));

            return 2;
        }

        /// <summary>
        /// #1933 Renamed module OrchardCore.Body to OrchardCore.Html.  This migration takes care of name
        /// changes in content definitions and content in database.  Thus, it needs to run only for existing
        /// installation with versions below 1.0.0-beta2-6646.
        /// </summary>
        public int UpdateFrom1()
        {
            using (var conn = _store.Configuration.ConnectionFactory.CreateConnection())
            {
                conn.Open();
                conn.Execute(RENAME_BODYPARTS_SQL);
            }
            return 2;
        }
    }
}