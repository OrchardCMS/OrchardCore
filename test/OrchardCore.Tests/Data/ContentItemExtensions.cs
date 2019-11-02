using System;
using OrchardCore.ContentFields.Fields;
using OrchardCore.ContentManagement;
using Xunit;

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
        public void GetContentItemIdFromTypedContentItem()
        {
            var contentItem = new ContentItem()
            {
                ContentItemId = "myid"
            };

            var typedItem = contentItem.To<CustomPartTypedContentItem>();
            Assert.NotNull(typedItem);
            Assert.Equal("myid", typedItem.ContentItemId);
        }

        [Fact]
        public void GetContentPartFromTypedContentItem()
        {
            var contentItem = new ContentItem();
            var part = contentItem.GetOrCreate<CustomPart>(nameof(CustomPart));
            part.Value = "assigned value";
            part.Apply();

            var typedItem = contentItem.To<CustomPartTypedContentItem>();
            Assert.NotNull(typedItem.CustomPart);
            Assert.Equal("assigned value", typedItem.CustomPart.Value);
        }

        [Fact]
        public void GetContentFieldFromTypedContentItem()
        {
            var contentItem = new ContentItem()
            {
                ContentType = "Foo"
            };

            // Create a field part to simulate holding the fields.
            var fieldPart = contentItem.GetOrCreate<ContentPart>(contentItem.ContentType);

            var part = fieldPart.GetOrCreate<TextField>(nameof(TextField));
            part.Text = "text value";
            fieldPart.Apply();

            var typedItem = contentItem.To<CustomFieldTypedContentItem>();
            Assert.NotNull(typedItem.TextField);
            Assert.Equal("text value", typedItem.TextField.Text);
        }


        [Fact]
        public void SetDisplayTextFromTypedContentItem()
        {
            var contentItem = new ContentItem();
            Assert.Null(contentItem.ContentItemId);

            var typedItem = contentItem.To<CustomPartTypedContentItem>();
            typedItem.DisplayText = "my title";
            Assert.Equal("my title", typedItem.DisplayText);
            Assert.Equal("my title", contentItem.DisplayText);
        }

        [Fact]
        public void SetContentPartValueFromTypedContentItem()
        {
            var contentItem = new ContentItem()
            {
                ContentType = "Foo"
            };

            // Create a field part to simulate holding the fields.
            var fieldPart = contentItem.GetOrCreate<ContentPart>(contentItem.ContentType);

            var part = fieldPart.GetOrCreate<CustomPart>(nameof(CustomPart));
            part.Value = "assigned value";
            part.Apply();
            var typedItem = contentItem.To<CustomPartTypedContentItem>();
            typedItem.CustomPart.Value = "new value";
            typedItem.CustomPart.Apply();

            var actual = contentItem.Get<CustomPart>(nameof(CustomPart));
            Assert.Equal("new value", actual.Value);
        }

        [Fact]
        public void ThrowsWhenTypedContentElementSet()
        {
            var contentItem = new ContentItem();
            var typedItem = contentItem.To<CustomBadPartTypedContentItem>();
            Assert.Throws<Exception>(() => typedItem.CustomPart = new CustomPart { Value = "assigned value" });
        }

        [Fact]
        public void ContentItemPropertiesCount()
        {
            var contentItem = typeof(ContentItem);
            var properties = contentItem.GetProperties();
            // If test fails, update TypedContentItemInterceptor dictionary with new property.
            Assert.Equal(14, properties.Length);
        }

        public class CustomPart : ContentPart
        {
            public string Value { get; set; }
            public TextField Field { get; set; }
        }

        public class CustomPartTypedContentItem : TypedContentItem
        {
            public CustomPartTypedContentItem(ContentItem contentItem) : base(contentItem) { }

            public virtual CustomPart CustomPart { get; }
        }

        public class CustomBadPartTypedContentItem : TypedContentItem
        {
            public CustomBadPartTypedContentItem(ContentItem contentItem) : base(contentItem) { }

            public virtual CustomPart CustomPart { get; set; }
        }

        public class CustomFieldTypedContentItem : TypedContentItem
        {
            public CustomFieldTypedContentItem(ContentItem contentItem) : base(contentItem) { }

            public virtual TextField TextField { get; }
        }
    }
}
