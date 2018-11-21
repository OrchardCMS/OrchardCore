using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.Data.Migration;

namespace OrchardCore.ReCaptcha.Forms
{
    public class Migrations : DataMigration
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public Migrations(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public int Create()
        {
            // NoCaptcha
            //_contentDefinitionManager.AlterPartDefinition("NoCaptchaPart", part => part
            //    .WithDescription("Provides captcha properties."));

            //_contentDefinitionManager.AlterTypeDefinition("NoCaptcha", type => type
            //    .WithPart("NoCaptchaPart")
            //    .Stereotype("Widget"));

            _contentDefinitionManager.AlterPartDefinition("ReCaptchaPart", part => part
                .WithDescription("Provides captcha properties."));

            _contentDefinitionManager.AlterTypeDefinition("ReCaptcha", type => type
                .WithPart("ReCaptchaPart")
                .Stereotype("Widget"));

            return 1;
        }
    }
}