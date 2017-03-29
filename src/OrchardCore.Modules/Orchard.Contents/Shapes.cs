using System;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display;
using Orchard.DisplayManagement;
using Orchard.DisplayManagement.Descriptors;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.Contents
{
    public class Shapes : IShapeTableProvider
    {
        public void Discover(ShapeTableBuilder builder)
        {
            builder.Describe("Content")
                .OnDisplaying(displaying =>
                {
                    ContentItem contentItem = displaying.Shape.ContentItem;
                    if (contentItem != null)
                    {
                        // Alternates in order of specificity. 
                        // Display type > content type > specific content > display type for a content type > display type for specific content
                        // BasicShapeTemplateHarvester.Adjust will then adjust the template name

                        // Content__[DisplayType] e.g. Content-Summary
                        displaying.ShapeMetadata.Alternates.Add("Content_" + EncodeAlternateElement(displaying.ShapeMetadata.DisplayType));

                        // Content__[ContentType] e.g. Content-BlogPost,
                        displaying.ShapeMetadata.Alternates.Add("Content__" + EncodeAlternateElement(contentItem.ContentType));

                        // Content__[Id] e.g. Content-42,
                        displaying.ShapeMetadata.Alternates.Add("Content__" + contentItem.Id);

                        // Content_[DisplayType]__[ContentType] e.g. Content-BlogPost.Summary
                        displaying.ShapeMetadata.Alternates.Add("Content_" + displaying.ShapeMetadata.DisplayType + "__" + EncodeAlternateElement(contentItem.ContentType));

                        // Content_[DisplayType]__[Id] e.g. Content-42.Summary
                        displaying.ShapeMetadata.Alternates.Add("Content_" + displaying.ShapeMetadata.DisplayType + "__" + contentItem.Id);
                    }
                });

            // This shapes provides a way to lazily load a content item render it in any display type.
            builder.Describe("ContentItem")
                .OnProcessingAsync(async context =>
                {
                    var content = context.Shape;
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

                    string contentItemId = await aliasManager.GetContentItemIdAsync(alias);

                    ContentItem contentItem = await contentManager.GetAsync(contentItemId);

                    if (contentItem == null)
                    {
                        return;
                    }

                    content.ContentItem = contentItem;

                    var displayShape = await displayManager.BuildDisplayAsync(contentItem, updateModelAccessor.ModelUpdater, displayType);

                    if (!String.IsNullOrEmpty(alternate))
                    {
                        ((IShape)displayShape).Metadata.Alternates.Add(displayShape);
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
            return alternateElement.Replace("-", "__").Replace(".", "_");
        }

    }
}
