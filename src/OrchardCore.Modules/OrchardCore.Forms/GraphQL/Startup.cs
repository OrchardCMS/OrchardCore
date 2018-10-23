using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Apis;
using OrchardCore.Forms.Models;
using OrchardCore.Modules;

namespace OrchardCore.Forms.GraphQL
{
    [RequireFeatures("OrchardCore.Apis.GraphQL")]
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddGraphQLQueryType<FormPart, FormPartQueryObjectType>();
            services.AddGraphQLQueryType<FormElementPart, FormElementPartQueryObjectType>();
            services.AddGraphQLQueryType<FormInputElementPart, FormInputElementPartQueryObjectType>();
            services.AddGraphQLQueryType<LabelPart, LabelPartQueryObjectType>();
            services.AddGraphQLQueryType<ButtonPart, ButtonPartQueryObjectType>();
            services.AddGraphQLQueryType<InputPart, InputPartQueryObjectType>();
            services.AddGraphQLQueryType<TextAreaPart, TextAreaPartQueryObjectType>();
            services.AddGraphQLQueryType<ValidationPart, ValidationPartQueryObjectType>();
            
            // Broken
            //services.AddGraphQLQueryType<ValidationSummaryPart, ValidationSummaryPartQueryObjectType>();
        }
    }
}