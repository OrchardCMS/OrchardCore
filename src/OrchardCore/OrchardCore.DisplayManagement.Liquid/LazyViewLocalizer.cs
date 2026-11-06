using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OrchardCore.Localization;

namespace OrchardCore.DisplayManagement.Liquid;

/// <summary>
/// An <see cref="IViewLocalizer"/> that is resolved and contextualized to its view on first use,
/// as most templates don't localize anything while contextualizing a localizer allocates.
/// </summary>
[SkipLocalizationExtraction]
internal sealed class LazyViewLocalizer : IViewLocalizer
{
    private readonly IServiceProvider _services;
    private readonly ViewContext _viewContext;

    private IViewLocalizer _localizer;

    public LazyViewLocalizer(IServiceProvider services, ViewContext viewContext)
    {
        _services = services;
        _viewContext = viewContext;
    }

    private IViewLocalizer Localizer
    {
        get
        {
            if (_localizer is null)
            {
                var localizer = _services.GetRequiredService<IViewLocalizer>();

                if (localizer is IViewContextAware contextable)
                {
                    contextable.Contextualize(GetPathViewContext(_viewContext));
                }

                _localizer = localizer;
            }

            return _localizer;
        }
    }

    // The view localizer is named after the path of the view, which a view context created to render a shape outside
    // a view doesn't have, e.g. by the IShapeHtmlRenderer: it's localized like the fallback view of the Liquid templates.
    private static ViewContext GetPathViewContext(ViewContext viewContext)
    {
        if (viewContext.View is null || !string.IsNullOrEmpty(viewContext.ExecutingFilePath) || !string.IsNullOrEmpty(viewContext.View.Path))
        {
            return viewContext;
        }

        return new ViewContext(viewContext, viewContext.View, viewContext.ViewData, viewContext.Writer)
        {
            ExecutingFilePath = LiquidViewsFeatureProvider.DefaultRazorViewPath,
        };
    }

    public LocalizedHtmlString this[string name] => Localizer[name];

    public LocalizedHtmlString this[string name, params object[] arguments] => Localizer[name, arguments];

    public LocalizedString GetString(string name) => Localizer.GetString(name);

    public LocalizedString GetString(string name, params object[] arguments) => Localizer.GetString(name, arguments);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Localizer.GetAllStrings(includeParentCultures);
}
