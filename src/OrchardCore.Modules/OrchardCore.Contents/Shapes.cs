using System;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.Contents
{
    public class Shapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content")
                .OnDisplaying(displaying =>
                {
                    dynamic shape = displaying.Shape;
                    ContentItem contentItem = shape.ContentItem;

                    if (contentItem != null)
                    {
                        // Alternates in order of specificity.
                        // Display type > content type > specific content > display type for a content type > display type for specific content
                        // BasicShapeTemplateHarvester.Adjust will then adjust the template name

                        // Content__[DisplayType] e.g. Content-Summary
                        displaying.Shape.Metadata.Alternates.Add("Content_" + EncodeAlternateElement(displaying.Shape.Metadata.DisplayType));

                        // Content__[ContentType] e.g. Content-BlogPost,
                        displaying.Shape.Metadata.Alternates.Add("Content__" + EncodeAlternateElement(contentItem.ContentType));

                        // Content__[Id] e.g. Content-42,
                        displaying.Shape.Metadata.Alternates.Add("Content__" + contentItem.Id);

                        // Content_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
                        displaying.Shape.Metadata.Alternates.Add("Content_" + displaying.Shape.Metadata.DisplayType + "__" + EncodeAlternateElement(contentItem.ContentType));

                        // Content_[DisplayType]__[Id] e.g. Content-42.Summary
                        displaying.Shape.Metadata.Alternates.Add("Content_" + displaying.Shape.Metadata.DisplayType + "__" + contentItem.Id);
                    }
                });

            // This shapes provides a way to lazily load a content item render it in any display type.
            builder.Describe("ContentItem")
                .OnProcessing(async context =>
                {
                    dynamic content = context.Shape;
                    string alias = content.Alias;
                    string displayType = content.DisplayType;
                    string alternate = content.Alternate;

                    if (String.IsNullOrEmpty(alias))
                    {
                        return;
                    }

                    var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                    var aliasManager = context.ServiceProvider.GetRequiredService<IContentAliasManager>();
                    var displayManager = context.ServiceProvider.GetRequiredService<IContentItemDisplayManager>();
                    var updateModelAccessor = context.ServiceProvider.GetRequiredService<IUpdateModelAccessor>();

                    var contentItemId = await aliasManager.GetContentItemIdAsync(alias);

                    if (string.IsNullOrEmpty(contentItemId))
                    {
                        return;
                    }

                    var contentItem = await contentManager.GetAsync(contentItemId);

                    if (contentItem == null)
                    {
                        return;
                    }

                    content.ContentItem = contentItem;

                    var displayShape = await displayManager.BuildDisplayAsync(contentItem, updateModelAccessor.ModelUpdater, displayType);

                    if (!String.IsNullOrEmpty(alternate))
                    {
                        displayShape.Metadata.Alternates.Add(alternate);
                    }

                    content.Add(displayShape);
                });
        }

        /// <summary>
        /// Encodes dashed and dots so that they don't conflict in filenames
        /// </summary>
        /// <param name="alternateElement"></param>
        /// <returns></returns>
        private string EncodeAlternateElement(string alternateElement)
        {
            return alternateElement.Replace("-", "__").Replace('.', '_');
        }
    }
}
