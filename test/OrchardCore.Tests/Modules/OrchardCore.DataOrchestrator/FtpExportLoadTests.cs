using OrchardCore.DataOrchestrator.Activities;

namespace OrchardCore.Tests.Modules.OrchardCore.DataOrchestrator;

public class FtpExportLoadTests
{
    [Fact]
    public void PasswordSetterStoresProtectedValue()
    {
        var load = new FtpExportLoad(new EphemeralDataProtectionProvider())
        {
            Password = "secret",
        };

        var storedPassword = load.Properties[nameof(FtpExportLoad.Password)].GetValue<string>();

        Assert.StartsWith("protected:", storedPassword);
        Assert.DoesNotContain("secret", storedPassword);
        Assert.Equal("secret", load.Password);
    }
}
