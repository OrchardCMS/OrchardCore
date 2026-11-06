using Fluid;
using OrchardCore.Liquid;

namespace OrchardCore.DisplayManagement.Liquid;

/// <summary>
/// Holds the Liquid template context shared by all the view templates rendered in the current scope, so that it
/// is initialized once per request instead of once per rendered template. Each template still renders in its own
/// child scope with its own model and view context.
/// </summary>
public sealed class LiquidTemplateContextAccessor
{
    private readonly IServiceProvider _serviceProvider;
    private LiquidTemplateContext _context;

    public LiquidTemplateContextAccessor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Gets the template context of the current scope, created on first use with the given options.
    /// </summary>
    public LiquidTemplateContext GetContext(TemplateOptions options)
        => _context ??= new LiquidTemplateContext(_serviceProvider, options);
}
