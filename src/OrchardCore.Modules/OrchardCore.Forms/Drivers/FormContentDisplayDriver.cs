using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.DisplayManagement.Handlers;
using System.Threading.Tasks;
using OrchardCore.Forms.Models;
namespace OrchardCore.Forms.Drivers
{
    public class FormContentDisplayDriver : ContentDisplayDriver
    {
        //private readonly IContentDefinitionManager _contentDefinitionManager;

        public FormContentDisplayDriver(IContentDefinitionManager contentDefinitionManager)
        {
            //_contentDefinitionManager = contentDefinitionManager;
        }

        public override Task<IDisplayResult> DisplayAsync(ContentItem model, BuildDisplayContext context)
        {
            var formItemShape = context.Shape;

            var formPart = model.As<FormPart>();
            if (formPart != null)
            {
                // var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.ContentType);
                context.Shape.Metadata.Wrappers.Add($"Form_Wrapper");
            }

            //We don't need to return a shape result
            return Task.FromResult<IDisplayResult>(null);
        }
    }
}
