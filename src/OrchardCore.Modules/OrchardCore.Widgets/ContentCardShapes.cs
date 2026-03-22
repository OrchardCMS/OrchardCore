using OrchardCore.ContentTypes.Shapes;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.Widgets;

public class ContentCardShapes : ShapeTableProvider
{
    // Card Shape
    private const string ContentCardEdit = "ContentCard_Edit";

    // Frame shape
    private const string ContentCardFrame = "ContentCard_Frame";

    // Card Editor Fields
    private const string ContentCardFieldsEdit = "ContentCard_Fields_Edit";

    public override ValueTask DiscoverAsync(ShapeTableBuilder builder)
    {
        builder.Describe(ContentCardEdit)
            .OnDisplaying(context =>
            {
                // Defines Edit Alternates for the Content Item being edited.
                var contentCardEditor = context.Shape;
                
                // Access properties - works with both dynamic and strongly typed shapes
                var collectionType = (contentCardEditor as ContentCardShape)?.CollectionShapeType 
                    ?? contentCardEditor.GetProperty<string>("CollectionShapeType");
                var contentType = (contentCardEditor as ContentCardShape)?.ContentTypeValue 
                    ?? contentCardEditor.GetProperty<string>("ContentTypeValue");
                var parentContentType = (contentCardEditor as ContentCardShape)?.ParentContentType 
                    ?? contentCardEditor.GetProperty<string>("ParentContentType");
                var namedPart = (contentCardEditor as ContentCardShape)?.CollectionPartName 
                    ?? contentCardEditor.GetProperty<string>("CollectionPartName");
                var buildEditor = (contentCardEditor as ContentCardShape)?.BuildEditor 
                    ?? contentCardEditor.TryGetProperty<bool>("BuildEditor", out var be) && be;
                    
                if (buildEditor)
                {
                    contentCardEditor.Metadata.Alternates.AddRange(ContentCardAlternatesFactory.GetEditAlternates(
                        collectionType,
                        contentType,
                        parentContentType,
                        namedPart));
                }
            });

        builder.Describe(ContentCardFrame)
            .OnDisplaying(context =>
            {
                // Alternates for Outer Frame of ContentCard
                var contentCardFrame = context.Shape;
                
                // Access ChildContent - works with both dynamic and strongly typed shapes
                var childContent = (contentCardFrame as ContentCardFrameShape)?.ChildContent 
                    ?? contentCardFrame.GetProperty<IShape>("ChildContent");
                    
                if (childContent == null)
                {
                    return;
                }
                
                // Access properties from ChildContent
                var collectionType = (childContent as ContentCardShape)?.CollectionShapeType 
                    ?? childContent.GetProperty<string>("CollectionShapeType");
                var contentType = (childContent as ContentCardShape)?.ContentTypeValue 
                    ?? childContent.GetProperty<string>("ContentTypeValue");
                var parentContentType = (childContent as ContentCardShape)?.ParentContentType 
                    ?? childContent.GetProperty<string>("ParentContentType");
                var namedPart = (childContent as ContentCardShape)?.CollectionPartName 
                    ?? childContent.GetProperty<string>("CollectionPartName");

                contentCardFrame.Metadata.Alternates.AddRange(ContentCardAlternatesFactory.GetFrameAlternates(
                    collectionType,
                    contentType,
                    parentContentType,
                    namedPart));
            });

        builder.Describe(ContentCardFieldsEdit)
           .OnDisplaying(context =>
           {
               var contentCardEditorFields = context.Shape;
               
               // Access CardShape property - works with both dynamic and strongly typed shapes
               var cardShape = (contentCardEditorFields as ContentCardFieldsEditShape)?.CardShape 
                   ?? contentCardEditorFields.GetProperty<IShape>("CardShape");
                   
               if (cardShape == null)
               {
                   return;
               }
               
               // Access CollectionShapeType from CardShape
               var collectionType = (cardShape as ContentCardShape)?.CollectionShapeType 
                   ?? cardShape.GetProperty<string>("CollectionShapeType");
                   
               if (!string.IsNullOrEmpty(collectionType))
               {
                   contentCardEditorFields.Metadata.Alternates.AddRange(ContentCardAlternatesFactory.GetFieldsEditAlternates(collectionType));
               }
           });

        return ValueTask.CompletedTask;
    }
}
