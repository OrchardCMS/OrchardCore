using OrchardCore.Users;
using OrchardCore.Users.Models;

namespace OrchardCore.Tests.OrchardCore.Users
{
    public class UserValidatorTests
    {
        [Fact]
        public async Task CanValidateUser()
        {
            // Arrange
            var userManager = UsersMockHelper.MockUserManager<IUser>();
            var user = new User { UserId = Guid.NewGuid().ToString("n"), UserName = "Foo", Email = "foo@foo.com" };
            userManager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.UserId);
            userManager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            userManager.Setup(m => m.GetEmailAsync(user)).ReturnsAsync(user.Email);

            // Act
            var validator = userManager.Object.UserValidators.FirstOrDefault();
            var result = await validator.ValidateAsync(userManager.Object, user);

            // Test
            Assert.True(result.Succeeded);
        }

        [Fact]
        public async Task ShouldRequireEmail()
        {
            // Arrange
            var describer = new IdentityErrorDescriber();
            var userManager = UsersMockHelper.MockUserManager<IUser>();
            var user = new User { UserId = Guid.NewGuid().ToString("n"), UserName = "Foo" };
            userManager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.UserId);
            userManager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            userManager.Setup(m => m.GetEmailAsync(user)).ReturnsAsync(user.Email);

            // Act
            var validator = userManager.Object.UserValidators.FirstOrDefault();
            var result = await validator.ValidateAsync(userManager.Object, user);

            // Test
            Assert.False(result.Succeeded);
            Assert.Equal(describer.InvalidEmail(user.Email).Code, result.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldRequireValidEmail()
        {
            // Arrange
            var describer = new IdentityErrorDescriber();
            var userManager = UsersMockHelper.MockUserManager<IUser>();
            var user = new User { UserId = Guid.NewGuid().ToString("n"), UserName = "Foo", Email = "foo" };
            userManager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.UserId);
            userManager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            userManager.Setup(m => m.GetEmailAsync(user)).ReturnsAsync(user.Email);

            // Act
            var validator = userManager.Object.UserValidators.FirstOrDefault();
            var result = await validator.ValidateAsync(userManager.Object, user);

            // Test
            Assert.False(result.Succeeded);
            Assert.Equal(describer.InvalidEmail(user.Email).Code, result.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldRequireUniqueEmail()
        {
            // Arrange
            var describer = new IdentityErrorDescriber();
            var userManager = UsersMockHelper.MockUserManager<IUser>();
            var existingUser = new User { UserId = Guid.NewGuid().ToString("n"), UserName = "Foo", Email = "foo@foo.com" };
            userManager.Setup(m => m.GetUserIdAsync(existingUser)).ReturnsAsync(existingUser.UserId);
            userManager.Setup(m => m.GetUserNameAsync(existingUser)).ReturnsAsync(existingUser.UserName);
            userManager.Setup(m => m.GetEmailAsync(existingUser)).ReturnsAsync(existingUser.Email);
            userManager.Setup(m => m.FindByEmailAsync(existingUser.Email)).ReturnsAsync(existingUser);
            userManager.Setup(m => m.FindByNameAsync(existingUser.UserName)).ReturnsAsync(existingUser);

            var user = new User { UserId = Guid.NewGuid().ToString("n"), UserName = "Bar", Email = "foo@foo.com" };
            userManager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.UserId);
            userManager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            userManager.Setup(m => m.GetEmailAsync(user)).ReturnsAsync(user.Email);

            // Act
            var validator = userManager.Object.UserValidators.FirstOrDefault();
            var result = await validator.ValidateAsync(userManager.Object, user);

            // Test
            Assert.False(result.Succeeded);
            Assert.Equal(describer.DuplicateEmail(user.Email).Code, result.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldRequireUserNameIsNotAnEmailAddress()
        {
            // Arrange
            var describer = new IdentityErrorDescriber();
            var userManager = UsersMockHelper.MockUserManager<IUser>();
            var user = new User { UserId = Guid.NewGuid().ToString("n"), UserName = "foo@foo.com", Email = "foo" };
            userManager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.UserId);
            userManager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            userManager.Setup(m => m.GetEmailAsync(user)).ReturnsAsync(user.Email);

            // Act
            var validator = userManager.Object.UserValidators.FirstOrDefault();
            var result = await validator.ValidateAsync(userManager.Object, user);

            // Test
            Assert.False(result.Succeeded);
            Assert.Equal(describer.InvalidUserName(user.UserName).Code, result.Errors.First().Code);
        }

        [Fact]
        public async Task ShouldRequireUniqueUserName()
        {
            // Arrange
            var describer = new IdentityErrorDescriber();
            var userManager = UsersMockHelper.MockUserManager<IUser>();
            var existingUser = new User { UserId = Guid.NewGuid().ToString("n"), UserName = "Foo", Email = "bar@bar.com" };
            userManager.Setup(m => m.GetUserIdAsync(existingUser)).ReturnsAsync(existingUser.UserId);
            userManager.Setup(m => m.GetUserNameAsync(existingUser)).ReturnsAsync(existingUser.UserName);
            userManager.Setup(m => m.GetEmailAsync(existingUser)).ReturnsAsync(existingUser.Email);
            userManager.Setup(m => m.FindByEmailAsync(existingUser.Email)).ReturnsAsync(existingUser);
            userManager.Setup(m => m.FindByNameAsync(existingUser.UserName)).ReturnsAsync(existingUser);

            var user = new User { UserId = Guid.NewGuid().ToString("n"), UserName = "Foo", Email = "foo@foo.com" };
            userManager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.UserId);
            userManager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            userManager.Setup(m => m.GetEmailAsync(user)).ReturnsAsync(user.Email);

            // Act
            var validator = userManager.Object.UserValidators.FirstOrDefault();
            var result = await validator.ValidateAsync(userManager.Object, user);

            // Test
            Assert.False(result.Succeeded);
            Assert.Equal(describer.DuplicateUserName(user.UserName).Code, result.Errors.First().Code);
        }
    }
}
