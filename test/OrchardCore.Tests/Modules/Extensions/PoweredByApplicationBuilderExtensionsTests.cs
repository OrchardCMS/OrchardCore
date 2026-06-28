namespace OrchardCore.Modules.Extensions.Tests;

public class PoweredByApplicationBuilderExtensionsTests
{
    [Fact]
    public async Task UsePoweredBy_ShouldAddDefaultHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var applicationBuilder = CreateApplicationBuilder();

        // Act
        applicationBuilder.UsePoweredBy();

        await applicationBuilder.Build().Invoke(context);

        // Assert
        Assert.Equal("OrchardCore", context.Response.Headers.XPoweredBy);
    }

    [Fact]
    public async Task UsePoweredBy_WithOptionsAction_ShouldAddConfiguredHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var applicationBuilder = CreateApplicationBuilder();

        // Act
        applicationBuilder.UsePoweredBy(options =>
        {
            options.HeaderName = "X-Custom-Powered-By";
            options.HeaderValue = "CustomValue";
        });

        await applicationBuilder.Build().Invoke(context);

        // Assert
        Assert.Equal("CustomValue", context.Response.Headers["X-Custom-Powered-By"]);
        Assert.False(context.Response.Headers.ContainsKey("X-Powered-By"));
    }

    [Fact]
    public void UsePoweredBy_ShouldThrowArgumentNullException_WhenAppIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>("app", () => PoweredByApplicationBuilderExtensions.UsePoweredBy(null!));
    }

    [Fact]
    public void UsePoweredBy_WithOptionsAction_ShouldThrowArgumentNullException_WhenAppIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>("app", () => PoweredByApplicationBuilderExtensions.UsePoweredBy(null!, _ => { }));
    }

    [Fact]
    public void UsePoweredBy_WithOptionsAction_ShouldThrowArgumentNullException_WhenOptionsActionIsNull()
    {
        // Arrange
        var applicationBuilder = CreateApplicationBuilder();

        // Act & Assert
        Assert.Throws<ArgumentNullException>("optionsAction", () => applicationBuilder.UsePoweredBy(null!));
    }

    private static ApplicationBuilder CreateApplicationBuilder()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IOptions<PoweredByOptions>>(new OptionsWrapper<PoweredByOptions>(new PoweredByOptions()));

        var serviceProvider = services.BuildServiceProvider();

        return new ApplicationBuilder(serviceProvider);
    }
}
