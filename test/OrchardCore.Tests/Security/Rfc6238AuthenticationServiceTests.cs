using OrchardCore.Modules;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Tests.Security;

public class Rfc6238AuthenticationServiceTests
{
    /*
    [Theory]
    [InlineData(180, 0, true)]
    [InlineData(180, 180, true)]
    [InlineData(180, 360, false)]
    [InlineData(180, 395, false)]
    [InlineData(180, 540, false)]
    [InlineData(240, 0, true)]
    [InlineData(240, 120, true)]
    [InlineData(240, 240, true)]
    [InlineData(240, 379, false)]
    [InlineData(240, 480, false)]
    [InlineData(240, 500, false)]
    public void ValidateCode_WhenCalled_ReturnsResult(int timeSpanInSecond, int validateAfterSeconds, bool isValid)
    {
        var timeSpan = TimeSpan.FromSeconds(timeSpanInSecond);
        var startTime = DateTime.UtcNow;

        var creationClock = new Mock<IClock>();
        creationClock.Setup(x => x.UtcNow)
            .Returns(startTime);

        var validationClock = new Mock<IClock>();
        validationClock.Setup(x => x.UtcNow)
            .Returns(startTime.AddSeconds(validateAfterSeconds));

        var factory = new Rfc6238AuthenticationService(timeSpan, TwoFactorEmailTokenLength.Eight, creationClock.Object);

        var validator = new Rfc6238AuthenticationService(timeSpan, TwoFactorEmailTokenLength.Eight, validationClock.Object);

        var modifier = "Totp:TwoFactor:1";

        var securityStamp = Encoding.Unicode.GetBytes("security-stamp");

        var pin = factory.GenerateCode(securityStamp, modifier);

        Assert.Equal(isValid, validator.ValidateCode(securityStamp, pin, modifier));
    }
    */

    [Theory]
    [InlineData(TwoFactorEmailTokenLength.Two)]
    [InlineData(TwoFactorEmailTokenLength.Three)]
    [InlineData(TwoFactorEmailTokenLength.Four)]
    [InlineData(TwoFactorEmailTokenLength.Five)]
    [InlineData(TwoFactorEmailTokenLength.Six)]
    [InlineData(TwoFactorEmailTokenLength.Seven)]
    [InlineData(TwoFactorEmailTokenLength.Eight)]
    [InlineData(TwoFactorEmailTokenLength.Default)]
    public void GetString_WhenCalled_ReturnsCorrectLength(TwoFactorEmailTokenLength length)
    {
        var timeSpan = TimeSpan.FromMinutes(5);

        var creationClock = new Mock<IClock>();
        creationClock.Setup(x => x.UtcNow)
            .Returns(DateTime.UtcNow);

        var factory = new Rfc6238AuthenticationService(timeSpan, length, creationClock.Object);

        var modifier = "Totp:TwoFactor:1";

        var securityStamp = Encoding.Unicode.GetBytes("security-stamp");

        if (length == TwoFactorEmailTokenLength.Eight || length == TwoFactorEmailTokenLength.Default)
        {
            var pin = factory.GenerateCode(securityStamp, modifier);

            Assert.Equal(8, factory.GetString(pin).Length);
        }
        else if (length == TwoFactorEmailTokenLength.Seven)
        {
            var pin = factory.GenerateCode(securityStamp, modifier);

            Assert.Equal(7, factory.GetString(pin).Length);
        }
        else if (length == TwoFactorEmailTokenLength.Six)
        {
            var pin = factory.GenerateCode(securityStamp, modifier);

            Assert.Equal(6, factory.GetString(pin).Length);
        }
        else if (length == TwoFactorEmailTokenLength.Five)
        {
            var pin = factory.GenerateCode(securityStamp, modifier);

            Assert.Equal(5, factory.GetString(pin).Length);
        }
        else if (length == TwoFactorEmailTokenLength.Four)
        {
            var pin = factory.GenerateCode(securityStamp, modifier);

            Assert.Equal(4, factory.GetString(pin).Length);
        }
        else if (length == TwoFactorEmailTokenLength.Three)
        {
            var pin = factory.GenerateCode(securityStamp, modifier);

            Assert.Equal(3, factory.GetString(pin).Length);
        }
        else if (length == TwoFactorEmailTokenLength.Two)
        {
            var pin = factory.GenerateCode(securityStamp, modifier);

            Assert.Equal(2, factory.GetString(pin).Length);
        }
        else
        {
            Assert.Throws<NotSupportedException>(() =>
            {
                factory.GenerateCode(securityStamp, modifier);
            });
        }
    }
}
