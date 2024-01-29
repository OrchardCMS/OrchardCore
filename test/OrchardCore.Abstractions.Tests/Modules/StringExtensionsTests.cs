using System;
using Xunit;

namespace OrchardCore.Modules
{
    public class StringExtensionsTests
    {
        [Theory]
        [InlineData(new byte[0], "")]
        [InlineData(new byte[] { 10, 20, 30 }, "0A141E")]
        public void ToHexString_ReturnsCorrectHexString(byte[] bytes, string expected)
        {
            // Arrange

            // Act
            var result = bytes.ToHexString();

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("414243", new byte[] { 0x41, 0x42, 0x43 })]
        [InlineData("48656C6C6F20576F726C64", new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F, 0x20, 0x57, 0x6F, 0x72, 0x6C, 0x64 })]
        [InlineData("", new byte[0])]
        public void ToByteArray_ShouldConvertToByteArray(string hex, byte[] expected)
        {
            // Arrange

            // Act
            var result = hex.ToByteArray();

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void ToByteArray_ShouldThrowException_WhenHexContainsOddNumberOfCharacters()
        {
            // Arrange
            var hex = "41424";

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => hex.ToByteArray());
        }

        [Fact]
        public void ToByteArray_ShouldThrowException_WhenHexIsNotValid()
        {
            // Arrange
            var hex = "GHIJK";

            // Act & Assert
            Assert.Throws<FormatException>(() => hex.ToByteArray());
        }
    }
}
