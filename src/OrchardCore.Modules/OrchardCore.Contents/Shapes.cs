using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Shapes;
using OrchardCore.DisplayManagement.Utilities;

namespace OrchardCore.Contents;

public class Shapes : ShapeTableProvider
{
    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe("Content")
            .OnDisplaying(displaying =>
            {
                var shape = displaying.Shape;
                var contentItem = shape.GetProperty<ContentItem>("ContentItem");

                if (contentItem != null)
                {
                    // Use cached alternates for content shapes and add them efficiently
                    var cachedAlternates = ShapeAlternatesFactory.GetContentAlternates(
                        contentItem.ContentType,
                        contentItem.Id.ToString());

                    var displayType = displaying.Shape.Metadata.DisplayType;
                    var alternates = cachedAlternates.GetAlternates(displayType);

                    displaying.Shape.Metadata.Alternates.AddRange(alternates);
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

                if (string.IsNullOrEmpty(handle))
                {
                    // This code is provided for backwards compatibility and can be removed in a future version.
                    handle = content.GetProperty<string>("Alias");
                    if (string.IsNullOrEmpty(handle))
                    {
                        return;
                    }
                }

                var contentManager = context.ServiceProvider.GetRequiredService<IContentManager>();
                var handleManager = context.ServiceProvider.GetRequiredService<IContentHandleManager>();
                var displayManager = context.ServiceProvider.GetRequiredService<IContentItemDisplayManager>();
                var updateModelAccessor = context.ServiceProvider.GetRequiredService<IUpdateModelAccessor>();

                var contentItemId = await handleManager.GetContentItemIdAsync(handle);

                if (string.IsNullOrEmpty(contentItemId))
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

                if (!string.IsNullOrEmpty(alternate))
                {
                    displayShape.Metadata.Alternates.Add(alternate);
                }

                await context.Shape.AddAsync(displayShape, string.Empty);
            });

        return ValueTask.CompletedTask;
    }
}
