using Fluid;
using OrchardCore.Environment.Shell;
using OrchardCore.Liquid;

namespace OrchardCore.Liquid.Abstractions;

public sealed class FluidOptionsParser<TOptions> where TOptions : class
{
    // Local instance since it can be discarded once the parsing is over.
    private readonly FluidParser _fluidParser;
    private readonly ShellSettings _shellSettings;

    private TemplateContext _templateContext;

    public FluidOptionsParser(
        FluidParser fluidParser,
        ShellSettings shellSettings)
    {
        _fluidParser = fluidParser;
        _shellSettings = shellSettings;
    }

    public string ParseAndFormat(string template)
    {
        var templateContext = GetTemplateContext();

        // Use Fluid directly as this is transient and cannot invoke _liquidTemplateManager.
        var parsedTemplate = _fluidParser.Parse(template);

        return parsedTemplate.Render(templateContext, NullEncoder.Default)
            .ReplaceLineEndings(string.Empty)
            .Trim();
    }

    private TemplateContext GetTemplateContext()
    {
        if (_templateContext == null)
        {
            var templateOptions = new TemplateOptionsBuilder()
                .WithMemberAccessStrategy<AllowListMemberAccessStrategy>()
                .ConfigureMemberAccess(strategy =>
                {
                    strategy.Register<ShellSettings>();
                    strategy.Register<TOptions>();
                })
                .Build();

            _templateContext = new TemplateContext(templateOptions);
            _templateContext.SetValue("ShellSettings", _shellSettings);
        }

        return _templateContext;
    }
}
