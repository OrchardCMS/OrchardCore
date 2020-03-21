using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.Models;
using OrchardCore.ContentManagement.Metadata.Models;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Metadata.Fields;
using OrchardCore.Metadata.Settings;
using OrchardCore.Metadata.ViewModels;
using OrchardCore.ResourceManagement;

namespace OrchardCore.Metadata.Drivers
{
    public class MetadataTextFieldDisplayDriver : ContentFieldDisplayDriver<MetadataTextField>
    {
        private readonly IStringLocalizer S;
        private readonly IResourceManager _resourceManager;


        public MetadataTextFieldDisplayDriver(IStringLocalizer<MetadataTextFieldDisplayDriver> localizer, IResourceManager resourceManager)
        {
            _resourceManager = resourceManager;
            S = localizer;
        }

        public override IDisplayResult Display(MetadataTextField field, BuildFieldDisplayContext context)
        {
            return null;
        }

        public override async Task<IDisplayResult> DisplayAsync(MetadataTextField field, BuildFieldDisplayContext context)
        {
            var settings = context.PartFieldDefinition.Settings.ToObject<MetadataTextFieldSettings>();

            if (context.DisplayType == "Detail")
            {
                switch (settings.DescriptorAttibuteType)
                {
                    case Models.AttributeType.name:
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Name = settings.Descriptor,
                            Content = field.Value
                        });
                        break;
                    case Models.AttributeType.property:
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Property = settings.Descriptor,
                            Content = field.Value
                        });
                        break;
                    case Models.AttributeType.notApplicable:
                        break;
                    default:
                        _resourceManager.RegisterMeta(new MetaEntry
                        {
                            Name = settings.Descriptor,
                            Content = field.Value
                        });
                        break;
                }
            }

            return await base.DisplayAsync(field, context);
        }

        public override IDisplayResult Edit(MetadataTextField field, BuildFieldEditorContext context)
        {
            return Initialize<EditMetadataTextFieldViewModel>(GetEditorShapeType(context), model =>
            {
                model.Value = field.Value;
                model.Field = field;
                model.Part = context.ContentPart;
                model.PartFieldDefinition = context.PartFieldDefinition;
            });
        }

        public override async Task<IDisplayResult> UpdateAsync(MetadataTextField field, IUpdateModel updater, UpdateFieldEditorContext context)
        {
            if (await updater.TryUpdateModelAsync(field, Prefix, f => f.Value))
            {
                var settings = context.PartFieldDefinition.GetSettings<MetadataTextFieldSettings>();
                if (settings.Required && String.IsNullOrWhiteSpace(field.Value))
                {
                    updater.ModelState.AddModelError(Prefix, S["A value is required for {0}.", context.PartFieldDefinition.DisplayName()]);
                }
            }

            return Edit(field, context);
        }
    }
}
