using Fluid;
using Fluid.Values;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Liquid;

namespace OrchardCore.Tests.DisplayManagement;

public class TemplateOptionsAccessorTests
{
    [Fact]
    public async Task Value_ConfiguredBuilder_AppliesTheConfiguration()
    {
        using var services = new ServiceCollection()
            .AddLiquidTemplateOptions()
            .Configure<TemplateOptionsBuilder>(options =>
            {
                options.AddFilter("shout", (input, _, context) => StringValue.Create(input.ToStringValue(context).ToUpperInvariant()));
                options.WithGlobalValue("Greeting", StringValue.Create("hello"));
                options.ConfigureMemberAccess(strategy => strategy.Register<Model, string>("Computed", (_, _) => "Value"));
            })
            .BuildServiceProvider();

        var options = services.GetRequiredService<IOptions<TemplateOptions>>().Value;

        Assert.True(new FluidParser().TryParse("{{ Greeting | shout }} {{ Model.Name }} {{ Model.Computed }}", out var template, out var error), error);

        var context = new TemplateContext(options);
        context.SetValue("Model", FluidValue.Create(new Model(), options));

        Assert.Equal("HELLO Name Value", await template.RenderAsync(context));
    }

    [Fact]
    public void Value_ReadTwice_IsBuiltOnce()
    {
        using var services = new ServiceCollection()
            .AddLiquidTemplateOptions()
            .BuildServiceProvider();

        Assert.Same(
            services.GetRequiredService<IOptions<TemplateOptions>>().Value,
            services.GetRequiredService<IOptions<TemplateOptions>>().Value);
    }

    [Fact]
    public void Value_DefaultStrategy_IsFluidDefault()
    {
        using var services = new ServiceCollection()
            .AddLiquidTemplateOptions()
            .BuildServiceProvider();

        var options = services.GetRequiredService<IOptions<TemplateOptions>>().Value;

        Assert.IsType<DefaultMemberAccessStrategy>(options.MemberAccessStrategy);
    }

    [Fact]
    public void Resolve_ConfigureTemplateOptions_Throws()
    {
        using var services = new ServiceCollection()
            .AddLiquidTemplateOptions()
            .Configure<TemplateOptions>(options => options.Filters.AddFilter("ignored", (input, _, _) => input))
            .BuildServiceProvider();

        var exception = Assert.Throws<InvalidOperationException>(() => services.GetRequiredService<IOptions<TemplateOptions>>());

        Assert.Contains(nameof(TemplateOptionsBuilder), exception.Message);
        Assert.Contains(nameof(TemplateOptionsAccessorTests), exception.Message);
    }

    public sealed class Model
    {
        public string Name { get; set; } = "Name";
    }
}
