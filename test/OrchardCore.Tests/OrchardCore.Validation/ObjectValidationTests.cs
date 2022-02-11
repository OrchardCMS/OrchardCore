using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Validation.Tests
{
    public class ObjectValidationTests
    {
        [Fact]
        public async Task ValidateObjectAsync()
        {
            // Arrange
            var errors = new List<ValidationResult>();
            var user = new User
            {
                UserName = "hishamco",
                Password = "admin@OC",
                ConfirmPassword = "admin@oc"
            };
            var validationContext = new ValidationContext(user);

            // Act
            var isValid = await ValidatorWrapper.TryValidateObjectAsync(user, validationContext, errors);

            // Assert
            Assert.False(isValid);
            Assert.NotEmpty(errors);
            Assert.Equal(2, errors.Count);
            Assert.Equal("The confirmation password doesn't match the password.", errors[0].ErrorMessage);
            Assert.Equal("The email is already taken.", errors[1].ErrorMessage);
        }

        [Fact]
        public void ValidateObject()
        {
            // Arrange
            var errors = new List<ValidationResult>();
            var person = new Person { FirstName = "Hisham" };
            var validationContext = new ValidationContext(person);

            // Act
            var isValid = ValidatorWrapper.TryValidateObject(person, validationContext, errors);

            // Assert
            Assert.False(isValid);
            Assert.Single(errors);
            Assert.Equal("The last name is required.", errors[0].ErrorMessage);
        }
    }
}
