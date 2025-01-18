using OrchardCore.Environment.Shell;
using OrchardCore.Localization;
using OrchardCore.Roles;

namespace OrchardCore.Tests.Security;

public class DefaultSystemRoleNameProviderTests
{
    [Fact]
    public async Task SystemRoleNamesContains_WhenConstructed_ContainsDefaultAdminRole()
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
        Assert.Contains(OrchardCoreConstants.Roles.Administrator, roles.Select(r => r.RoleName));
    }

    [Fact]
    public async Task SystemRoleNamesContains_WhenConstructed_ContainsConfiguredAdminRole()
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
        var roles = await provider.GetSystemRolesAsync();

        // Assert
        var roleNames = roles.Select(r => r.RoleName);
        Assert.Contains(configureSystemAdminRoleName, roleNames);
        Assert.DoesNotContain(OrchardCoreConstants.Roles.Administrator, roleNames);
    }

    [Fact]
    public async Task SystemRoleNamesContains_WhenConstructed_ContainsAppSettingsRole()
    {
        // Arrange
        var localizer = Mock.Of<IStringLocalizer<DefaultSystemRoleProvider>>();
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = "Foo";

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(o => o.Value)
            .Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleProvider(shellSettings, localizer, options.Object);

        // Act
        var roles = await provider.GetSystemRolesAsync();

        // Assert
        var roleNames = roles.Select(r => r.RoleName);
        Assert.DoesNotContain(OrchardCoreConstants.Roles.Administrator, roleNames);
        Assert.Contains("Foo", roleNames);
    }
}
