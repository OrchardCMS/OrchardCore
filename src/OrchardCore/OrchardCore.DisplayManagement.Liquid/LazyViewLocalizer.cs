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
                    contextable.Contextualize(_viewContext);
                }

                _localizer = localizer;
            }

            return _localizer;
        }
    }

    public LocalizedHtmlString this[string name] => Localizer[name];

    public LocalizedHtmlString this[string name, params object[] arguments] => Localizer[name, arguments];

    public LocalizedString GetString(string name) => Localizer.GetString(name);

    public LocalizedString GetString(string name, params object[] arguments) => Localizer.GetString(name, arguments);

    public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) => Localizer.GetAllStrings(includeParentCultures);
}
