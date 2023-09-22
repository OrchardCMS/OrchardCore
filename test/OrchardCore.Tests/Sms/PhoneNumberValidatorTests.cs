namespace OrchardCore.Sms.Tests
{
    public class PhoneNumberValidatorTests
    {
        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("+967777198442", true)]
        [InlineData("7777198442", false)]
        public void ValidateEmailAddress(string address, bool isValid)
        {
            // Arrange
            var phoneNumberValidator = new PhoneNumberValidator();

            // Act & Assert
            Assert.Equal(isValid, phoneNumberValidator.Validate(address));
        }

        [Theory]
        [InlineData("00966777198442", false)]
        [InlineData("00967777198442", true)]
        public void UseCustomPhoneNumberValidator(string address, bool isValid)
        {
            // Arrange
            var phoneNumberValidator = new YemenPhoneNumberValidator();

            // Act & Assert
            Assert.Equal(isValid, phoneNumberValidator.Validate(address));
        }

        private class YemenPhoneNumberValidator : IPhoneNumberValidator
        {
            public bool Validate(string phoneNumber)
                => phoneNumber.Length == 14 && phoneNumber.StartsWith("00967");
        }
    }
}
