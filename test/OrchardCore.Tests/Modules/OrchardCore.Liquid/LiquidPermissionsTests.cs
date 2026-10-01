using OrchardCore.Liquid;

namespace OrchardCore.Tests.Modules.OrchardCore.Liquid;

public class LiquidPermissionsTests
{
    [Fact]
    public async Task ManageLiquidTemplatesShouldBeSecurityCriticalAndGrantedToEditors()
    {
        var provider = new Permissions();

        var permission = Assert.Single(await provider.GetPermissionsAsync());
        Assert.Equal(Permissions.ManageLiquidTemplates, permission);
        Assert.True(permission.IsSecurityCritical);

        var stereotypes = provider.GetDefaultStereotypes().ToDictionary(stereotype => stereotype.Name);
        Assert.Contains(
            Permissions.ManageLiquidTemplates,
            stereotypes[OrchardCoreConstants.Roles.Administrator].Permissions);
        Assert.Contains(
            Permissions.ManageLiquidTemplates,
            stereotypes[OrchardCoreConstants.Roles.Editor].Permissions);
    }
}
