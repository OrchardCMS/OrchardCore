# Data Migrations (`OrchardCore.Data.Migration.DataMigration`)

Data Migration classes can be used to alter the content type definitions (like by adding new __types__, or configuring their __parts__ and __fields__), 
initializing recipes or creating indices.

There should be one Migrations file per module inheriting from __DataMigration__. However, if a module has more than one feature and multiple migrations per feature, each migration should be decorated with the Feature attribute i.e. `[Feature("OrchardCore.ContentFields.Indexing.SQL")]`

Initial migration method should be named `public int Create` or `public Task<int> CreateAsync` and it should return a number (like  `return 1`).
Any subsequent migration should follow the convention `public int UpdateFromX` or `public Task<int> UpdateFromXAsync`, where __X__ is the number returned from the last migration method. Migrations are strictly chained:
eg. UpdateFrom4 will only be executed if the last executed migration returned the number 4. You should not revise these numbers after the fact because it could break migrations for other people.

Migrations are executed automatically on application start.

The following example showcases three different data migrations (recipe migration, creating a map index, creating content type and updating content type).

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
                // Make sure to set column length, otherwise migration will not work for all databases
                table.Column<string>(nameof(MemberIndex.SocialSecurityNumber), column => column.WithLength(11))
                .Column<string>(nameof(MemberIndex.Name), column => column.WithLength(26))
                .Column<string>(nameof(MemberIndex.Surname), column => column.WithLength(26))
            });
            
            // This will create index on the sql table itself, 
            // which will make fetches by 'SocialSecurityNumber' column faster
            SchemaBuilder.AlterIndexTable<MemberIndex>(table => table
                .CreateIndex("IDX_MemberIndex_SocialSecurityNumber",
                    "SocialSecurityNumber")
            );
            return 2;
        }

        public int UpdateFrom2()
        {
            _contentDefinitionManager.AlterTypeDefinition("Product", type => type
                // Content items of this type can have drafts
                .Draftable()
                // Content items versions of this type are versionable
                .Versionable()
                // This content type appears in the New menu section
                .Creatable()
                // Permissions can be applied specifically to instances of this type
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

## Additional information
Please refer to separate sections for more details on data migrations:

- [Data Migration of Content Types](../ContentTypes/README.md#migrations)
- [Data Migration of Recipes](../Recipes/README.md#recipe-migrations)
