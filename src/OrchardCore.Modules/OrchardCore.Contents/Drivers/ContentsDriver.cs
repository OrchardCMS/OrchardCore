using System;
using System.Collections.Generic;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Display.ViewModels;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.ContentManagement.Metadata.Settings;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Contents.Drivers
{
    public class ContentsDriver : ContentDisplayDriver
    {
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public ContentsDriver(IContentDefinitionManager contentDefinitionManager)
        {
            _contentDefinitionManager = contentDefinitionManager;
        }

        public override IDisplayResult Display(ContentItem model, IUpdateModel updater)
        {
            // We add custom alternates. This could be done generically to all shapes coming from ContentDisplayDriver but right now it's
            // only necessary on this shape. Otherwise c.f. ContentPartDisplayDriver

            var contentsMetadataShape = Shape("ContentsMetadata", new ContentItemViewModel(model)).Location("Detail", "Content:before");

            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(model.ContentType);

            if (contentTypeDefinition != null)
            {
                contentsMetadataShape.Displaying(ctx =>
                {
                    var stereotype = "";
                    var settings = contentTypeDefinition?.GetSettings<ContentTypeSettings>();
                    if (settings != null)
                    {
                        stereotype = settings.Stereotype;
                    }

                    if (!String.IsNullOrEmpty(stereotype) && !String.Equals("Content", stereotype, StringComparison.OrdinalIgnoreCase))
                    {
                        ctx.Shape.Metadata.Alternates.Add($"{stereotype}__ContentsMetadata");
                    }
                });
            }

            return Combine(
                contentsMetadataShape,
                Shape("Contents_SummaryAdmin__Tags", new ContentItemViewModel(model)).Location("SummaryAdmin", "Tags:10"),
                Shape("Contents_SummaryAdmin__Meta", new ContentItemViewModel(model)).Location("SummaryAdmin", "Meta:20"),
                Shape("Contents_SummaryAdmin__Button__Edit", new ContentItemViewModel(model)).Location("SummaryAdmin", "Actions:10"),
                Shape("Contents_SummaryAdmin__Button__Actions", new ContentItemViewModel(model)).Location("SummaryAdmin", "Actions:20")
            );
        }

        public override IDisplayResult Edit(ContentItem contentItem, IUpdateModel updater)
        {
            var contentTypeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (contentTypeDefinition == null)
            {
                return null;
            }

            var results = new List<IDisplayResult>
            {
                Dynamic("Content_PublishButton").Location("Actions:10"),
            };

            if (contentTypeDefinition.GetSettings<ContentTypeSettings>().Draftable)
            {
                results.Add(Dynamic("Content_SaveDraftButton").Location("Actions:20"));
            }

            return Combine(results.ToArray());
        }
    }
}
