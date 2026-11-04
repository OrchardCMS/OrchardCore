using Fluid;
using Fluid.Values;
using OrchardCore.Liquid;

namespace OrchardCore.Tests.DisplayManagement;

public class AllowListMemberAccessStrategyTests
{
    private static readonly FluidParser s_parser = new();

    [Fact]
    public async Task Render_UnregisteredType_DoesNotExposeItsMembers()
    {
        var options = new TemplateOptionsBuilder()
            .WithMemberAccessStrategy<AllowListMemberAccessStrategy>()
            .ConfigureMemberAccess(strategy => strategy.Register<RegisteredModel>())
            .Build();

        var result = await RenderAsync("[{{ Model.Name }}][{{ Model.Secret.Value }}][{{ Secret.Value }}]", options);

        Assert.Equal("[Name][][]", result);
    }

    [Fact]
    public void GetAccessor_RegisteredType_ExposesPublicInstanceMembersOnly()
    {
        var strategy = new AllowListMemberAccessStrategy();

        strategy.Register<RegisteredModel>();

        Assert.NotNull(strategy.GetAccessor(typeof(RegisteredModel), nameof(RegisteredModel.Name), StringComparer.Ordinal));
        Assert.NotNull(strategy.GetAccessor(typeof(RegisteredModel), nameof(RegisteredModel.Field), StringComparer.Ordinal));
        Assert.Null(strategy.GetAccessor(typeof(RegisteredModel), nameof(RegisteredModel.Static), StringComparer.Ordinal));
        Assert.Null(strategy.GetAccessor(typeof(UnregisteredModel), nameof(UnregisteredModel.Value), StringComparer.Ordinal));
    }

    [Fact]
    public void GetAccessor_DifferentCasing_ReturnsNull()
    {
        var strategy = new AllowListMemberAccessStrategy();

        strategy.Register<RegisteredModel>();

        Assert.Null(strategy.GetAccessor(typeof(RegisteredModel), "name", StringComparer.OrdinalIgnoreCase));
    }

    [Fact]
    public void GetAccessor_DerivedType_UsesBaseTypeAndInterfaceRegistrations()
    {
        var strategy = new AllowListMemberAccessStrategy();

        strategy.Register<RegisteredModel>();
        strategy.Register<INamed>();
        strategy.Register<DerivedModel, string>((obj, name) => name);

        Assert.NotNull(strategy.GetAccessor(typeof(DerivedModel), nameof(RegisteredModel.Name), StringComparer.Ordinal));
        Assert.NotNull(strategy.GetAccessor(typeof(MoreDerivedModel), "Dynamic", StringComparer.Ordinal));
        Assert.NotNull(strategy.GetAccessor(typeof(NamedModel), nameof(INamed.Title), StringComparer.Ordinal));
    }

    [Fact]
    public void Register_AfterAMemberWasRead_Throws()
    {
        var strategy = new AllowListMemberAccessStrategy();

        strategy.Register<RegisteredModel>();
        strategy.GetAccessor(typeof(RegisteredModel), nameof(RegisteredModel.Name), StringComparer.Ordinal);

        Assert.Throws<InvalidOperationException>(() => strategy.Register<UnregisteredModel>());
    }

    private static async Task<string> RenderAsync(string source, TemplateOptions options)
    {
        Assert.True(s_parser.TryParse(source, out var template, out var error), error);

        var context = new TemplateContext(options);

        context.SetValue("Model", FluidValue.Create(new RegisteredModel(), options));
        context.SetValue("Secret", FluidValue.Create(new UnregisteredModel(), options));

        return await template.RenderAsync(context);
    }

    public interface INamed
    {
        string Title { get; }
    }

    public class RegisteredModel
    {
        public static string Static { get; } = "Static";

        public string Name { get; set; } = "Name";

        public string Field = "Field";

        public UnregisteredModel Secret { get; set; } = new UnregisteredModel();
    }

    public class DerivedModel : RegisteredModel;

    public class MoreDerivedModel : DerivedModel;

    public class NamedModel : INamed
    {
        public string Title => "Title";
    }

    public class UnregisteredModel
    {
        public string Value { get; set; } = "Secret value";
    }
}
