using OrchardCore.DisplayManagement.Shapes;

namespace OrchardCore.Tests.DisplayManagement
{
    public class NilTests
    {
        [Fact]
        public void NilShouldEqualToNull()
        {
            var nil = Nil.Instance;

            Assert.True(nil is null);
            Assert.False(nil is not null);

            Assert.True(nil == Nil.Instance);
            Assert.False(nil != Nil.Instance);
        }

        [Fact]
        public void NilShouldBeRecursive()
        {
            dynamic nil = Nil.Instance;

            Assert.True(nil is null);
            Assert.True(nil.Foo is null);
            Assert.True(nil.Foo.Bar is null);
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
            Assert.True((string)nil is null);
        }
    }
}
