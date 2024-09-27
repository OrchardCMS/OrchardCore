using OrchardCore.Environment.Shell;
using OrchardCore.Roles;

namespace OrchardCore.Tests.Security;

public class SystemRolesCatalogTests
{
    [Fact]
    public void SystemRoleNamesContains_WhenConstructed_ContainsDefaultAdminRole()
    {
        // Arrange
        var shellSettings = new ShellSettings();

        var catalog = new SystemRolesCatalog(shellSettings);

        // Assert
        Assert.Contains(OrchardCoreConstants.Roles.Administrator, catalog.SystemRoleNames as IEnumerable<string>);
    }

    [Fact]
    public void SystemRoleNamesContains_WhenConstructed_ContainsAppSettingsRole()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = "Foo";
        var catalog = new SystemRolesCatalog(shellSettings);

        // Assert
        Assert.DoesNotContain(OrchardCoreConstants.Roles.Administrator, catalog.SystemRoleNames as IEnumerable<string>);
        Assert.Contains("Foo", catalog.SystemRoleNames as IEnumerable<string>);
    }

    [Fact]
    public void SystemRoleNamesContains_WhenCalled_ReturnsCaseInsensitive()
    {
        // Arrange
        var shellSettings = new ShellSettings();
        shellSettings["AdminRoleName"] = "Foo";
        var catalog = new SystemRolesCatalog(shellSettings);

        // Assert
        Assert.Contains("fOo", catalog.SystemRoleNames as IEnumerable<string>);
    }
}
