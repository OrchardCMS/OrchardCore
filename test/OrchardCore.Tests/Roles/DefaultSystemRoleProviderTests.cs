using OrchardCore.Environment.Shell;

namespace OrchardCore.Roles.Tests;

public class DefaultSystemRoleProviderTests
{
    [Fact]
    public async Task GetSystemRoles_Have_Administrator_Authenticated_Anonymous()
    {
        // Arrange
        var localizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();
        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, localizer, options.Object);

        // Act
        var roles = await provider.GetSystemRolesAsync();

        // Assert
        var roleNames = roles.Select(r => r.RoleName);
        Assert.Contains(OrchardCoreConstants.Roles.Administrator, roleNames);
        Assert.Contains(OrchardCoreConstants.Roles.Authenticated, roleNames);
        Assert.Contains(OrchardCoreConstants.Roles.Anonymous, roleNames);
    }

    [Fact]
    public async Task GetAdminRole_FromConfiguredAdminRole()
    {
        // Arrange
        var localizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();
        var configureSystemAdminRoleName = "SystemAdmin";
        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions
            {
                SystemAdminRoleName = configureSystemAdminRoleName,
            });

        var provider = new DefaultSystemRoleProvider(shellSettings, localizer, options.Object);

        // Act
        var role = await provider.GetAdminRoleAsync();

        // Assert
        Assert.Equal(configureSystemAdminRoleName, role.RoleName);
        Assert.NotEqual(OrchardCoreConstants.Roles.Administrator, role.RoleName);
    }

    [Fact]
    public async Task GetAdminRole_FromAppSettings()
    {
        // Arrange
        var adminRoleName = "Foo";
        var localizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = adminRoleName;

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, localizer, options.Object);

        // Act
        var role = await provider.GetAdminRoleAsync();

        // Assert
        Assert.Equal(adminRoleName, role.RoleName);
        Assert.NotEqual(OrchardCoreConstants.Roles.Administrator, role.RoleName);
    }
}
