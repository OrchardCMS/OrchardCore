using OrchardCore.Environment.Shell;

namespace OrchardCore.Roles.Tests;

public class SystemRoleProviderExtensionsTests
{
    [Theory]
    [InlineData("Administrator", true)]
    [InlineData("ADMINISTRATOR", true)]
    [InlineData("Authenticated", true)]
    [InlineData("authenticated", true)]
    [InlineData("Anonymous", true)]
    [InlineData("AnonYmouS", true)]
    [InlineData("Test", false)]
    [InlineData("TEST", false)]
    [InlineData("TesT", false)]
    [InlineData("test", false)]
    public async Task IsSystemRole_ReturnsTrue_IfTheRoleExists(string roleName, bool expectedResult)
    {
        // Arrange
        var localizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        // Act
        var provider = new DefaultSystemRoleProvider(shellSettings, localizer, options.Object);

        // Assert
        Assert.Equal(expectedResult, await provider.IsSystemRoleAsync(roleName));
    }

    [Theory]
    [InlineData("Administrator", true)]
    [InlineData("ADMINISTRATOR", true)]
    [InlineData("administrator", true)]
    [InlineData("AdminiSTratoR", true)]
    [InlineData("Test", false)]
    [InlineData("TEST", false)]
    [InlineData("TesT", false)]
    [InlineData("test", false)]
    public async Task IsAdminRole_ReturnsTrue_IfTheRoleExists(string roleName, bool expectedResult)
    {
        // Arrange
        var localizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        // Act
        var provider = new DefaultSystemRoleProvider(shellSettings, localizer, options.Object);

        // Assert
        Assert.Equal(expectedResult, await provider.IsAdminRoleAsync(roleName));
    }
}
