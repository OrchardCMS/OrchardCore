using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;

namespace OrchardCore.Tests.Data
{
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
        public void MergeReflectsChangesToWellKnownProperties()
        {
            // Setup
            var contentItem = new ContentItem
            {
                DisplayText = "original value"
            };

            var newContentItem = new ContentItem
            {
                DisplayText = "merged value"
            };

            // Act
            contentItem.Merge(newContentItem);

            // Test
            var content = (JObject)contentItem.Content;
            Assert.False(content.ContainsKey(nameof(contentItem.DisplayText)));
        }

        [Fact]
        public void MergeRemovesWellKnownPropertiesFromData()
        {
            // Setup
            var contentItem = new ContentItem
            {
                DisplayText = "original value"
            };

            var newContentItem = new ContentItem
            {
                DisplayText = "merged value"
            };

            // Act
            contentItem.Merge(newContentItem);

            // Test
            var content = (JObject)contentItem.Content;
            Assert.False(content.ContainsKey(nameof(contentItem.DisplayText)));
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
                Id = 2
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
}
