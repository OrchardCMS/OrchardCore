namespace OrchardCore.Sms.Tests;

public class DefaultPhoneFormatValidatorTests
{
    [Theory]
    [InlineData("+1-541-754-3010", true)]
    [InlineData("541-754-3010", false)]
    [InlineData("5417543010", false)]
    [InlineData("+967777198442", true)]
    [InlineData("+967-777198442", true)]
    [InlineData("00967777198442", false)]
    [InlineData("777198442", false)]
    public void ValidatePhoneNumber(string phoneNumber, bool expectedResult)
    {
        // Arrange
        var validator = new DefaultPhoneFormatValidator();

        // Act
        var result = validator.IsValid(phoneNumber);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
