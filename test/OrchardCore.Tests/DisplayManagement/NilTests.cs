using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.DisplayManagement
{
    public class NilTests
    {
        [Fact]
        public void NilShouldEqualToNull()
        {
            var nil = Nil.Instance;

            Assert.True(nil == null);
            Assert.False(nil != null);

            Assert.True(nil == Nil.Instance);
            Assert.False(nil != Nil.Instance);
        }

        [Fact]
        public void NilShouldBeRecursive()
        {
            dynamic nil = Nil.Instance;

            Assert.True(nil == null);
            Assert.True(nil.Foo == null);
            Assert.True(nil.Foo.Bar == null);
        }

        [Fact]
        public void CallingToStringOnNilShouldReturnEmpty()
        {
            var nil = Nil.Instance;
            Assert.Equal("", nil.ToString());
        }

        [Fact]
        public void CallingToStringOnDynamicNilShouldReturnEmpty()
        {
            dynamic nil = Nil.Instance;
            Assert.Equal("", nil.Foo.Bar.ToString());
        }

        [Fact]
        public void ConvertingToStringShouldReturnNullString()
        {
            dynamic nil = Nil.Instance;
            Assert.True((string)nil == null);
        }
    }
}
