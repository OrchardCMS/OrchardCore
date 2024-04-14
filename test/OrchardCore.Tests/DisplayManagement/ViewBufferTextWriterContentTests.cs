using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Liquid;

namespace OrchardCore.Tests.DisplayManagement
{
    public class ViewBufferTextWriterContentTests
    {
        private static string Serialize(ViewBufferTextWriterContent buffer)
        {
            using var sw = new StringWriter();
            buffer.WriteTo(sw, HtmlEncoder.Default);
            return sw.ToString();
        }

        [Fact]
        public void ShouldWriteString()
        {
            var buffer = new ViewBufferTextWriterContent();

            buffer.Write("<div>");

            var result = Serialize(buffer);

            Assert.Equal("<div>", result);
        }

        [Fact]
        public void ShouldWriteChar()
        {
            var buffer = new ViewBufferTextWriterContent();

            buffer.Write('a');

            var result = Serialize(buffer);

            Assert.Equal("a", result);
        }

        [Fact]
        public void ShouldWriteBufferFragment()
        {
            var buffer = new ViewBufferTextWriterContent();

            buffer.Write("abcd".ToCharArray(), 1, 2);

            var result = Serialize(buffer);

            Assert.Equal("bc", result);
        }

        [Fact]
        public void ShouldWriteBuffer()
        {
            var buffer = new ViewBufferTextWriterContent();

            buffer.Write("abcd".ToCharArray());

            var result = Serialize(buffer);

            Assert.Equal("abcd", result);
        }

        [Fact]
        public void ShouldWriteObject()
        {
            var buffer = new ViewBufferTextWriterContent();

            buffer.Write((object)"abcd");

            var result = Serialize(buffer);

            Assert.Equal("abcd", result);
        }

        [Fact]
        public void ShouldWriteMultipleFragments()
        {
            var buffer = new ViewBufferTextWriterContent();

            buffer.Write("ab");
            buffer.Write("cd");

            var result = Serialize(buffer);

            Assert.Equal("abcd", result);
        }

        [Fact]
        public void ShouldWriteMultipleStringPages()
        {
            var buffer = new ViewBufferTextWriterContent();

            var capacity = StringBuilderPool.GetInstance().Builder.Capacity;
            var page = new string('x', capacity);

            buffer.Write(page);
            buffer.Write(page);
            var result = Serialize(buffer);

            Assert.Equal(page + page, result);
        }

        [Fact]
        public void ShouldWriteMultipleCharPages()
        {
            var buffer = new ViewBufferTextWriterContent();

            var capacity = StringBuilderPool.GetInstance().Builder.Capacity;

            buffer.Write(new string('x', capacity - 1));
            buffer.Write('x');
            buffer.Write('x');
            var result = Serialize(buffer);

            Assert.Equal(capacity + 1, result.Length);
        }

        [Fact]
        public void ShouldWriteMultipleBufferFragmentPages()
        {
            var buffer = new ViewBufferTextWriterContent();

            var capacity = StringBuilderPool.GetInstance().Builder.Capacity;

            buffer.Write(new string('x', capacity - 1).ToCharArray());
            buffer.Write(new string('x', 11).ToCharArray(), 1, 3);
            var result = Serialize(buffer);

            Assert.Equal(capacity + 2, result.Length);
        }
    }
}
