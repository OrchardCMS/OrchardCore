using OrchardCore.Email;

namespace OrchardCore.Tests.Email
{
    public class EmailAddressValidatorTests
    {
        [Theory]
        [InlineData("mailbox@domain.com", true)]
        [InlineData("mailbox_domain.com", false)]
        [InlineData("mailbox@domain", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void ValidateEmailAddress(string address, bool isValid)
        {
            // Arrange
            var emailAddressValidator = new EmailAddressValidator();

            // Act & Assert
            Assert.Equal(isValid, emailAddressValidator.Validate(address));
        }

        [Theory]
        [InlineData("hishamco_2007@hotmail.com", false)]
        [InlineData("hishamco@orchardcore.net", true)]
        public void UseCustomEmailAddressValidator(string address, bool isValid)
        {
            // Arrange
            var emailAddressValidator = new OrchardCoreEmailValidator();

            // Act & Assert
            Assert.Equal(isValid, emailAddressValidator.Validate(address));
        }

        private class OrchardCoreEmailValidator : IEmailAddressValidator
        {
            public bool Validate(string emailAddress)
                => emailAddress.EndsWith("@orchardcore.net");
        }
    }
}
