using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;
using OrchardCore.Security;
using OrchardCore.Users;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace OrchardCore.Tests.OrchardCore.Users.Core
{
    public class DefaultUserClaimsPrincipalFactoryTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task EnsurePrincipalHasExpectedClaims(bool emailVerified)
        {
            //Arrange
            var userManager = UsersMockHelper.MockUserManager<IUser>();
            var user = new User { Id = new Random().Next(), UserName = "Foo", Email = "foo@foo.com" };
            userManager.Setup(m => m.GetUserIdAsync(user)).ReturnsAsync(user.Id.ToString());
            userManager.Setup(m => m.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            userManager.Setup(m => m.GetEmailAsync(user)).ReturnsAsync(user.Email);
            if (emailVerified)
            {
                userManager.Setup(m => m.IsEmailConfirmedAsync(user)).ReturnsAsync(emailVerified);
            }

            var roleManager = UsersMockHelper.MockRoleManager<IRole>().Object;

            var options = new Mock<IOptions<IdentityOptions>>();
            options.Setup(a => a.Value).Returns(new IdentityOptions());
            
            var factory = new DefaultUserClaimsPrincipalFactory(userManager.Object, roleManager, options.Object);

            //Act
            var principal = await factory.CreateAsync(user);

            //Assert
            var identity = principal.Identities.First();
            Assert.NotNull(identity);

            var claims = identity.Claims.ToList();
            Assert.NotNull(claims);

            var emailClaim = claims.FirstOrDefault(c => c.Type == "email");
            Assert.NotNull(emailClaim);
            Assert.Equal(user.Email, emailClaim.Value);

            var emailVerifiedClaim = claims.FirstOrDefault(c => c.Type == "email_verified");
            Assert.NotNull(emailVerifiedClaim);
            Assert.Equal(ClaimValueTypes.Boolean, emailVerifiedClaim.ValueType);
            Assert.Equal(emailVerified.ToString(), emailVerifiedClaim.Value); 
        }
    }

}
