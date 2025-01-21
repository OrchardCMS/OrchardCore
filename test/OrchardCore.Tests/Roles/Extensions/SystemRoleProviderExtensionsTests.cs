using OrchardCore.Environment.Shell;

namespace OrchardCore.Roles.Tests;

public class SystemRoleProviderExtensionsTests
{
    [Theory]
    [InlineData("Administrator", true)]
    [InlineData("ADMINISTRATOR", true)]
    [InlineData("administrator", true)]
    [InlineData("AdminiSTratoR", true)]
    [InlineData("Test", false)]
    [InlineData("TEST", false)]
    [InlineData("TesT", false)]
    [InlineData("test", false)]
    public void IsAdminRole_ReturnsTrue_IfTheRoleExists(string roleName, bool expectedResult)
    {
        // Arrange
        var stringLocalizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        // Act
        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object, stringLocalizer);

        // Assert
        Assert.Equal(expectedResult, provider.IsAdminRole(roleName));
    }
}
