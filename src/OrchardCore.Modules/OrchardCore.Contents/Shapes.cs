using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Contents
{
    public class Shapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content")
                .OnDisplaying(displaying =>
                {
                    var shape = displaying.Shape;
                    var contentItem = shape.GetProperty<ContentItem>("ContentItem");

                    if (contentItem != null)
                    {
                        // Alternates in order of specificity.
                        // Display type > content type > specific content > display type for a content type > display type for specific content
                        // BasicShapeTemplateHarvester.Adjust will then adjust the template name

                        // Content__[DisplayType] e.g. Content-Summary
                        displaying.Shape.Metadata.Alternates.Add("Content_" + displaying.Shape.Metadata.DisplayType.EncodeAlternateElement());

                        var encodedContentType = contentItem.ContentType.EncodeAlternateElement();

                        // Content__[ContentType] e.g. Content-BlogPost,
                        displaying.Shape.Metadata.Alternates.Add("Content__" + encodedContentType);

                        // Content__[Id] e.g. Content-42,
                        displaying.Shape.Metadata.Alternates.Add("Content__" + contentItem.Id);

                        // Content_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
                        displaying.Shape.Metadata.Alternates.Add("Content_" + displaying.Shape.Metadata.DisplayType + "__" + encodedContentType);

                        // Content_[DisplayType]__[Id] e.g. Content-42.Summary
                        displaying.Shape.Metadata.Alternates.Add("Content_" + displaying.Shape.Metadata.DisplayType + "__" + contentItem.Id);
                    }
                });

            // This shapes provides a way to lazily load a content item render it in any display type.
            builder.Describe("ContentItem")
                .OnProcessing(async context =>
                {
                    var content = context.Shape;
                    var handle = content.GetProperty<string>("Handle");
                    var displayType = content.GetProperty<string>("DisplayType");
                    var alternate = content.GetProperty<string>("Alternate");

                    if (String.IsNullOrEmpty(handle))
                    {
                        // This code is provided for backwards compatibility and can be removed in a future version.
                        handle = content.GetProperty<string>("Alias");
                        if (String.IsNullOrEmpty(handle))
                        {
                            return;
                        }
                    }

                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                    var handleManager = context.ServiceProvider.GetRequiredService<IContentHandleManager>();
                    var displayManager = context.ServiceProvider.GetRequiredService<IContentItemDisplayManager>();
                    var updateModelAccessor = context.ServiceProvider.GetRequiredService<IUpdateModelAccessor>();

                    var contentItemId = await handleManager.GetContentItemIdAsync(handle);

                    if (String.IsNullOrEmpty(contentItemId))
                    {
                        return;
                    }

                    var contentItem = await contentManager.GetAsync(contentItemId);

                    if (contentItem == null)
                    {
                        return;
                    }

                    content.Properties["ContentItem"] = contentItem;

                    var displayShape = await displayManager.BuildDisplayAsync(contentItem, updateModelAccessor.ModelUpdater, displayType);

                    if (!String.IsNullOrEmpty(alternate))
                    {
                        displayShape.Metadata.Alternates.Add(alternate);
                    }

                    await context.Shape.AddAsync(displayShape, "");
                });
        }
    }
}
