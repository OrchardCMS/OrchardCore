using OrchardCore.Environment.Shell;
using OrchardCore.Roles;

namespace OrchardCore.Tests.Security;

public class DefaultSystemRoleNameProviderTests
{
    [Fact]
    public async Task SystemRoleNamesContains_WhenConstructed_ContainsDefaultAdminRole()
    {
        // Arrange
        var shellSettings = new ShellSettings();

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(x => x.Value).Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleNameProvider(shellSettings, options.Object);

        // Assert
        var roles = await provider.GetSystemRolesAsync();

        Assert.Contains(OrchardCoreConstants.Roles.Administrator, roles as IEnumerable<string>);
    }

    [Fact]
    public async Task SystemRoleNamesContains_WhenConstructed_ContainsConfiguredAdminRole()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        var configureSystemAdminRoleName = "SystemAdmin";

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(x => x.Value).Returns(new SystemRoleOptions
        {
            SystemAdminRoleName = configureSystemAdminRoleName,
        });

        var provider = new DefaultSystemRoleNameProvider(shellSettings, options.Object);

        // Assert
        var roles = await provider.GetSystemRolesAsync();

        Assert.Contains(configureSystemAdminRoleName, roles as IEnumerable<string>);
        Assert.DoesNotContain(OrchardCoreConstants.Roles.Administrator, roles as IEnumerable<string>);
    }

    [Fact]
    public async Task SystemRoleNamesContains_WhenConstructed_ContainsAppSettingsRole()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = "Foo";

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(x => x.Value).Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleNameProvider(shellSettings, options.Object);
        // Assert
        var roles = await provider.GetSystemRolesAsync();

        Assert.DoesNotContain(OrchardCoreConstants.Roles.Administrator, roles as IEnumerable<string>);
        Assert.Contains("Foo", roles as IEnumerable<string>);
    }

    [Fact]
    public async Task SystemRoleNamesContains_WhenCalled_ReturnsCaseInsensitive()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = "Foo";

        var options = new Mock<IOptions<SystemRoleOptions>>();
        options.Setup(x => x.Value).Returns(new SystemRoleOptions());

        var provider = new DefaultSystemRoleNameProvider(shellSettings, options.Object);

        // Assert
        var roles = await provider.GetSystemRolesAsync();

        Assert.Contains("fOo", roles as IEnumerable<string>);
    }
}
