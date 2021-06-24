# Data Migrations (`OrchardCore.Data.Migration.DataMigration`)

Data Migration classes can be used to alter the content type definitions (like by adding new __types__, or configuring their __parts__ and __fields__), 
initializing recipes or creating indices.

There should be one Migrations file per module inheriting from __DataMigration__. 

Initial migration method should be named __Create__ or __CreateAsync__ and it should return a number (like  `return 1`).
Any subsequent migration should follow the convention `UpdateFromX`, where __X__ is the number returned from the last migration method. Migrations are strictly chained:
eg. UpdateFrom4 will only be executed if the last executed migration returned that number 4. You should not revise these numbers after the fact because it could break migrations for other people.

Migrations are executed automatically on application start.

The following example showcases three different data migrations (recipe migration, creating index, creating content type and updating content type).

```csharp
using System.Threading.Tasks;
using Members.Indexes;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.Recipes.Services;
using YesSql.Sql;

namespace Members
{
    public class Migrations : DataMigration
    {
        private readonly IRecipeMigrator _recipeMigrator;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IRecipeMigrator recipeMigrator, IContentDefinitionManager contentDefinitionManager)
        {
            _recipeMigrator = recipeMigrator;
            _contentDefinitionManager = contentDefinitionManager;
        }

        public async Task<int> CreateAsync()
        {
            await _recipeMigrator.ExecuteAsync("init.recipe.json", this);

            return 1;
        }

        public int UpdateFrom1()
        {
            SchemaBuilder.CreateMapIndexTable<MemberIndex>(table =>
            {
                table.Column<string>(nameof(MemberIndex.Oib));
                table.Column<string>(nameof(MemberIndex.Name));
                table.Column<string>(nameof(MemberIndex.Surname));
            });
            return 2;
        }

        public int UpdateFrom2()
        {
            _contentDefinitionManager.AlterTypeDefinition("Product", type => type
                // content items of this type can have drafts
                .Draftable()
                // content items versions of this type have saved
                .Versionable()
                // this content type appears in the New menu section
                .Creatable()
                // permissions can be applied specifically to instances of this type
                .Securable()
            );
            return 3;
        }

        public int UpdateFrom3()
        {
            _contentDefinitionManager.AlterTypeDefinition("Product", type => type
                .WithPart("TitlePart")
            );
            return 4;
        }

    }
}
```
