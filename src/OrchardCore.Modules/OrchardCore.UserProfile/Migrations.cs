using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;
using OrchardCore.UserProfile.Indexes;
using OrchardCore.UserProfile.Models;

namespace OrchardCore.UserProfile
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
            SchemaBuilder.CreateMapIndexTable(nameof(UserProfileIndex), table => table
                .Column<string>("TimeZone")
            );

            //_contentDefinitionManager.AlterPartDefinition(typeof(Models.UserProfile).Name, builder => builder
            //    .WithDescription("Provide ability to the application's users to complete a profile with names and picture")
            //    .Attachable(false)
            //);

            //_contentDefinitionManager.AlterTypeDefinition("User", builder => builder
            //    .WithPart(typeof(Models.UserProfile).Name)
            //);

            

            return 1;
        }
    }
}