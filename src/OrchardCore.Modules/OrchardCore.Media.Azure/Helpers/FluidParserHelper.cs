using Fluid;
using OrchardCore.Environment.Shell;

namespace OrchardCore.Media.Azure.Helpers;

internal class FluidParserHelper<TOptions> where TOptions : class
{
    // Local instance since it can be discarded once the startup is over.
    private readonly FluidParser _fluidParser = new();
    private readonly ShellSettings _shellSettings;

    private TemplateContext _templateContext;

    public FluidParserHelper(ShellSettings shellSettings)
    {
        _shellSettings = shellSettings;
    }

    public string ParseAndFormat(string template)
    {
        EnsureInitialized();

        // Use Fluid directly as this is transient and cannot invoke _liquidTemplateManager.
        var parsedTemplate = _fluidParser.Parse(template);
        return parsedTemplate.Render(_templateContext, NullEncoder.Default)
            .Replace("\r", string.Empty)
            .Replace("\n", string.Empty);
    }

    private void EnsureInitialized()
    {
        if (_templateContext != null)
        {
            return;
        }

        var templateOptions = new TemplateOptions();
        _templateContext = new TemplateContext(templateOptions);
        templateOptions.MemberAccessStrategy.Register<ShellSettings>();
        templateOptions.MemberAccessStrategy.Register<TOptions>();
        _templateContext.SetValue("ShellSettings", _shellSettings);
    }
}
