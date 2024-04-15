using Fluid;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Media.Azure.Helpers;

// This is almost the same as in OrchardCore.Media.Azure but there isn't really a good common place for it.
internal sealed class OptionsFluidParserHelper<TOptions> where TOptions : class
{
    // Local instance since it can be discarded once the startup is over.
    private readonly FluidParser _fluidParser = new();
    private readonly ShellSettings _shellSettings;

    private TemplateContext _templateContext;

    public OptionsFluidParserHelper(ShellSettings shellSettings)
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
            .Replace("\n", string.Empty);
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
