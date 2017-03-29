using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentFields.Fields;
using Orchard.ContentFields.Indexing;
using Orchard.ContentFields.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentTypes.Editors;
using Orchard.Indexing;

namespace Orchard.ContentFields
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // Boolean Field
            services.AddSingleton<ContentField, BooleanField>();
            services.AddScoped<IContentFieldDisplayDriver, BooleanFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, BooleanFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, BooleanFieldIndexHandler>();

            // Text Field
            services.AddSingleton<ContentField, TextField>();
            services.AddScoped<IContentFieldDisplayDriver, TextFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, TextFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, TextFieldIndexHandler>();

            // Html Field
            services.AddSingleton<ContentField, HtmlField>();
            services.AddScoped<IContentFieldDisplayDriver, HtmlFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, HtmlFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, HtmlFieldIndexHandler>();

            // Link Field
            services.AddSingleton<ContentField, LinkField>();
            services.AddScoped<IContentFieldDisplayDriver, LinkFieldDisplayDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, LinkFieldSettingsDriver>();
            services.AddScoped<IContentFieldIndexHandler, LinkFieldIndexHandler>();
        }
    }
}
