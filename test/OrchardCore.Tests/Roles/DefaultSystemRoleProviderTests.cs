using OrchardCore.Environment.Shell;

namespace OrchardCore.Roles.Tests;

public class DefaultSystemRoleProviderTests
{
    [Fact]
    public void GetSystemRoles_WhenCalledByDefault_ContainsAdministratorAuthenticatedAndAnonymousRoles()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object);

        // Act
        var roles = provider.GetSystemRoles();

        // Assert
        var roleNames = roles.Select(r => r.RoleName);
        Assert.Contains(OrchardCoreConstants.Roles.Administrator, roleNames);
        Assert.Contains(OrchardCoreConstants.Roles.Authenticated, roleNames);
        Assert.Contains(OrchardCoreConstants.Roles.Anonymous, roleNames);
    }

    [Fact]
    public void GetAdminRole_WhenCalled_UseSystemRoleOptions()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        var configureSystemAdminRoleName = "SystemAdmin";
        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions
            {
                SystemAdminRoleName = configureSystemAdminRoleName,
            });

        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object);

        // Act
        var role = provider.GetAdminRole();

        // Assert
        Assert.Equal(configureSystemAdminRoleName, role.RoleName);
        Assert.NotEqual(OrchardCoreConstants.Roles.Administrator, role.RoleName);
    }

    [Fact]
    public void GetAdminRole_WhenCalled_UseAdminRoleNameFromTheTenantSettings()
    {
        // Arrange
        var adminRoleName = "Foo";
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = adminRoleName;

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
        .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object);

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
    public void IsSystemRole_WhenCalled_ReturnsTrueIfTheRoleIsSystemRole(string roleName, bool expectedResult)
    {
        // Arrange
        var shellSettings = new ShellSettings();

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object);

        // Act
        var result = provider.IsSystemRole(roleName);

        // Assert
        Assert.Equal(expectedResult, result);
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
    public void IsAdminRole_WhenCalled_ReturnsAdministrator(string roleName, bool expectedResult)
    {
        // Arrange
        var shellSettings = new ShellSettings();

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, options.Object);

        // Act
        var result = provider.IsAdminRole(roleName);

        // Assert
        Assert.Equal(expectedResult, result);
    }
}
