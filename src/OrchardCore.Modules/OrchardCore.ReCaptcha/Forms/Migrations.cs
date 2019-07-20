using System.Threading.Tasks;
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

        public async Task<int> CreateAsync()
        {
            // NoCaptcha
            //await _contentDefinitionManager.AlterPartDefinitionAsync("NoCaptchaPart", part => part
            //    .WithDescription("Provides captcha properties."));

            //await _contentDefinitionManager.AlterTypeDefinitionAsync("NoCaptcha", type => type
            //    .WithPart("NoCaptchaPart")
            //    .Stereotype("Widget"));

            await _contentDefinitionManager.AlterPartDefinitionAsync("ReCaptchaPart", part => part
                .WithDescription("Provides captcha properties."));

            await _contentDefinitionManager.AlterTypeDefinitionAsync("ReCaptcha", type => type
                .WithPart("ReCaptchaPart")
                .Stereotype("Widget"));

            return 1;
        }
    }
}
