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

        var provider = new DefaultSystemRoleNameProvider(shellSettings);

        // Assert
        var roles = await provider.GetSystemRolesAsync();

        Assert.Contains(OrchardCoreConstants.Roles.Administrator, roles as IEnumerable<string>);
    }

    [Fact]
    public async Task SystemRoleNamesContains_WhenConstructed_ContainsAppSettingsRole()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = "Foo";
        var provider = new DefaultSystemRoleNameProvider(shellSettings);

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

        var provider = new DefaultSystemRoleNameProvider(shellSettings);

        // Assert
        var roles = await provider.GetSystemRolesAsync();

        Assert.Contains("fOo", roles as IEnumerable<string>);
    }
}
