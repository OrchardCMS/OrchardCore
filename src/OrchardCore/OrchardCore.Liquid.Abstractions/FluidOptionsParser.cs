using Fluid;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Liquid.Abstractions;

public sealed class FluidOptionsParser<TOptions> where TOptions : class
{
    // Local instance since it can be discarded once the parsing is over.
    private readonly FluidParser _fluidParser = new();
    private readonly ShellSettings _shellSettings;

    private TemplateContext _templateContext;

    public FluidOptionsParser(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public string ParseAndFormat(string template)
    {
        var templateContext = GetTemplateContext();

        // Use Fluid directly as this is transient and cannot invoke _liquidTemplateManager.
        var parsedTemplate = _fluidParser.Parse(template);
        return parsedTemplate.Render(templateContext, NullEncoder.Default)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty)
            .Trim();
    }

    private TemplateContext GetTemplateContext()
    {
        if (_templateContext == null)
        {
            var templateOptions = new TemplateOptions();
            _templateContext = new TemplateContext(templateOptions);
            templateOptions.MemberAccessStrategy.Register<ShellSettings>();
            templateOptions.MemberAccessStrategy.Register<TOptions>();
            _templateContext.SetValue("ShellSettings", _shellSettings);
        }

        return _templateContext;
    }
}
