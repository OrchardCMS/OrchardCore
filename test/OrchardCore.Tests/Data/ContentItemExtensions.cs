using System.Text.Json.Nodes;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Tests.Data;

public class ContentItemExtensions
{
    [Fact]
    public void GetReflectsChangesMadeByApply()
    {
        // arrange
        var value = "changed value";
        var contentItem = new ContentItem();
        contentItem.Weld<CustomPart>();
        var part = new CustomPart { Value = value };

        // act
        contentItem.Apply(nameof(CustomPart), part);

        // assert
        var actual = contentItem.Get<CustomPart>(nameof(CustomPart));
        Assert.Equal(actual.Value, value);
    }

    [Fact]
    public void GetReflectsChangesToFieldMadeByApply()
    {
        // arrange
        var value = "changed value";
        var contentItem = new ContentItem();
        contentItem.Weld<CustomPart>();
        var part = contentItem.Get<CustomPart>(nameof(CustomPart));
        var field = part.GetOrCreate<TextField>(nameof(CustomPart.Field));
        field.Text = value;

        // act
        part.Apply(nameof(CustomPart.Field), field);

        // assert
        var actual = contentItem.Get<CustomPart>(nameof(CustomPart));
        Assert.Equal(actual.Field?.Text, value);
    }

    [Fact]
    public void WeldPreservesFieldsWhenPartWeldedAfterFields()
    {
        // arrange
        var firstValue = "Hello";
        var secondValue = "World";
        var contentItem = new ContentItem { ContentType = "Product" };
        var productPart = new ContentPart();
        
        // Weld fields to the part first
        productPart.Weld("First", new TextField { Text = firstValue });
        productPart.Weld("Second", new TextField { Text = secondValue });
        
        // act - weld the part to the content item
        contentItem.Weld("ProductPart", productPart);
        
        // assert - fields should be preserved
        var retrievedPart = contentItem.Get<ContentPart>("ProductPart");
        Assert.NotNull(retrievedPart);
        
        var firstField = retrievedPart.Get<TextField>("First");
        Assert.NotNull(firstField);
        Assert.Equal(firstValue, firstField.Text);
        
        var secondField = retrievedPart.Get<TextField>("Second");
        Assert.NotNull(secondField);
        Assert.Equal(secondValue, secondField.Text);
    }

    [Fact]
    public void WeldPreservesFieldsWhenPartWeldedBeforeFields()
    {
        // arrange
        var firstValue = "Hello";
        var secondValue = "World";
        var contentItem = new ContentItem { ContentType = "Product" };
        var productPart = new ContentPart();
        
        // Weld the part to the content item first
        contentItem.Weld("ProductPart", productPart);
        
        // act - weld fields to the part after
        var retrievedPart = contentItem.Get<ContentPart>("ProductPart");
        retrievedPart.Weld("First", new TextField { Text = firstValue });
        retrievedPart.Weld("Second", new TextField { Text = secondValue });
        
        // assert - fields should be preserved
        var finalPart = contentItem.Get<ContentPart>("ProductPart");
        Assert.NotNull(finalPart);
        
        var firstField = finalPart.Get<TextField>("First");
        Assert.NotNull(firstField);
        Assert.Equal(firstValue, firstField.Text);
        
        var secondField = finalPart.Get<TextField>("Second");
        Assert.NotNull(secondField);
        Assert.Equal(secondValue, secondField.Text);
    }

    [Fact]
    public void MergeReflectsChangesToWellKnownProperties()
    {
        // Setup
        var contentItem = new ContentItem
        {
            DisplayText = "original value",
        };

        var newContentItem = new ContentItem
        {
            DisplayText = "merged value",
        };

        // Act
        contentItem.Merge(newContentItem);

        // Test
        Assert.False(contentItem.Content.ContainsKey(nameof(contentItem.DisplayText)));
    }

    [Fact]
    public void MergeRemovesWellKnownPropertiesFromData()
    {
        // Setup
        var contentItem = new ContentItem
        {
            DisplayText = "original value",
        };

        var newContentItem = new ContentItem
        {
            DisplayText = "merged value",
        };

        // Act
        contentItem.Merge(newContentItem);

        // Test
        Assert.False(contentItem.Content.ContainsKey(nameof(contentItem.DisplayText)));
    }

    [Fact]
    public void MergeMaintainsDocumentId()
    {
        // Setup
        var contentItem = new ContentItem
        {
            Id = 1,
        };

        var newContentItem = new ContentItem
        {
            Id = 2,
        };

        // Act
        contentItem.Merge(newContentItem);

        // Test
        Assert.Equal(contentItem.Id, contentItem.Id);
    }

    [Fact]
    public void MergeReflectsChangesToElements()
    {
        // Setup
        var value = "merged value";
        var contentItem = new ContentItem();

        var newContentItem = new ContentItem();
        newContentItem.Weld<CustomPart>();
        newContentItem.Alter<CustomPart>(x => x.Value = value);

        // Act
        contentItem.Merge(newContentItem);

        // Test
        var mergedPart = contentItem.As<CustomPart>();
        Assert.Equal(value, mergedPart?.Value);
    }

    public class CustomPart : ContentPart
    {
        public string Value { get; set; }
        public TextField Field { get; set; }
    }
}
