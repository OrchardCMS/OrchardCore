using OrchardCore.Environment.Shell;

namespace OrchardCore.Roles.Tests;

public class DefaultSystemRoleProviderTests
{
    [Fact]
    public void GetSystemRoles_Have_Administrator_Authenticated_Anonymous()
    {
        // Arrange
        var stringLocalizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();
        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object, stringLocalizer);

        // Act
        var roles = provider.GetSystemRoles();

        // Assert
        var roleNames = roles.Select(r => r.RoleName);
        Assert.Contains(OrchardCoreConstants.Roles.Administrator, roleNames);
        Assert.Contains(OrchardCoreConstants.Roles.Authenticated, roleNames);
        Assert.Contains(OrchardCoreConstants.Roles.Anonymous, roleNames);
    }

    [Fact]
    public void GetAdminRole_FromConfiguredAdminRole()
    {
        // Arrange
        var stringLocalizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();
        var configureSystemAdminRoleName = "SystemAdmin";
        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions
            {
                SystemAdminRoleName = configureSystemAdminRoleName,
            });

        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object, stringLocalizer);

        // Act
        var role = provider.GetAdminRole();

        // Assert
        Assert.Equal(configureSystemAdminRoleName, role.RoleName);
        Assert.NotEqual(OrchardCoreConstants.Roles.Administrator, role.RoleName);
    }

    [Fact]
    public void GetAdminRole_FromAppSettings()
    {
        // Arrange
        var adminRoleName = "Foo";
        var stringLocalizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = adminRoleName;

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
        .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object, stringLocalizer);

        // Act
        var role = provider.GetAdminRole();

        // Assert
        Assert.Equal(adminRoleName, role.RoleName);
        Assert.NotEqual(OrchardCoreConstants.Roles.Administrator, role.RoleName);
    }

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
    public void IsSystemRole_ReturnsTrue_IfTheRoleExists(string roleName, bool expectedResult)
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
        Assert.Equal(expectedResult, provider.IsSystemRole(roleName));
    }
}
