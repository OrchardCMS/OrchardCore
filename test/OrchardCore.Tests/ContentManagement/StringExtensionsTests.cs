using FluentAssertions;
using OrchardCore.ContentManagement.Utilities;

namespace OrchardCore.ContentManagement.Abstractions
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData("helloWorld", "hello World")]
        public void CamelFriendly_ReturnsExpectedResult(string source, string expected)
        {
            var result = source.CamelFriendly();

            result.Should().Be(expected);
        }
    }
}
