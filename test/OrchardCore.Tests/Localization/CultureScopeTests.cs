using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Builders;
using OrchardCore.Environment.Shell.Models;
using OrchardCore.Localization;
using OrchardCore.Locking;
using OrchardCore.Locking.Distributed;
using Xunit;

namespace OrchardCore.Tests.Localization;

public class CultureScopeTests
{
    [Fact]
    public Task CultureScopeSetUICultureAutomaticallyIfNotSet()
    {
        // Arrange
        var culture = "ar-YE";

        // Act
        return CreateShellContext().CreateScope().UsingAsync(scope =>
        {
            using (var cultureScope = CultureScope.Create(culture))
            {
                    // Assert
                    Assert.Equal(culture, cultureScope.Culture.Name);
                Assert.Equal(culture, cultureScope.UICulture.Name);
            }

            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CultureScopeRetrievesBothCultureAndUICulture()
    {
        // Arrange
        var culture = "ar";
        var uiCulture = "ar-YE";

        // Act
        await CreateShellContext().CreateScope().UsingAsync(async scope =>
        {
            using (var cultureScope = CultureScope.Create(culture, uiCulture))
            {
                    // Assert
                    Assert.Equal(culture, cultureScope.Culture.Name);
                Assert.Equal(uiCulture, cultureScope.UICulture.Name);
            }

            await Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CultureScopeRetrievesTheOrginalCulturesAfterScopeEnded()
    {
        // Arrange
        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;

        // Act & Assert
        await CreateShellContext().CreateScope().UsingAsync(async scope =>
        {
            using (var cultureScope = CultureScope.Create("FR"))
            {

            }

            Assert.Equal(culture, CultureInfo.CurrentCulture);
            Assert.Equal(uiCulture, CultureInfo.CurrentUICulture);

            await Task.CompletedTask;
        });
    }

    [Fact]
    public async Task CultureScopeRetrievesTheOrginalCulturesIfExceptionOccurs()
    {
        // Arrange
        var culture = CultureInfo.CurrentCulture;
        var uiCulture = CultureInfo.CurrentUICulture;

        // Act & Assert
        await CreateShellContext().CreateScope().UsingAsync(async scope =>
        {
            await Assert.ThrowsAsync<Exception>(() =>
            {
                using (var cultureScope = CultureScope.Create("FR"))
                {
                    throw new Exception("Something goes wrong!!");
                }
            });

            Assert.Equal(culture, CultureInfo.CurrentCulture);
            Assert.Equal(uiCulture, CultureInfo.CurrentUICulture);
        });
    }

    private static ShellContext CreateShellContext() => new()
    {
        Settings = new ShellSettings() { Name = ShellHelper.DefaultShellName, State = TenantState.Running },
        ServiceProvider = CreateServiceProvider(),
    };

    private static IServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton<IDistributedLock, LocalLock>();
        services.AddScoped<ILocalizationService, DefaultLocalizationService>();

        return services.BuildServiceProvider();
    }
}
