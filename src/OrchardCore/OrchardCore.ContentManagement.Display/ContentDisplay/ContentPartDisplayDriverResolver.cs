using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OrchardCore.ContentManagement.Display.ContentDisplay;

public class ContentPartDisplayDriverResolver : IContentPartDisplayDriverResolver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ContentDisplayOptions _contentDisplayOptions;

    // The drivers are scoped services, they are resolved once per scope as they are requested for each displayed content item.
    private readonly Dictionary<(string Name, string DisplayMode), IContentPartDisplayDriver[]> _displayModeDrivers = [];

    public ContentPartDisplayDriverResolver(
        IServiceProvider serviceProvider,
        IOptions<ContentDisplayOptions> contentDisplayOptions
        )
    {
        _serviceProvider = serviceProvider;
        _contentDisplayOptions = contentDisplayOptions.Value;
    }

    public IList<IContentPartDisplayDriver> GetDisplayModeDrivers(string partName, string displayMode)
    {
        if (_displayModeDrivers.TryGetValue((partName, displayMode), out var drivers))
        {
            return drivers;
        }

        var services = new List<IContentPartDisplayDriver>();

        if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
        {
            foreach (var displayDriverOption in contentPartDisplayOption.DisplayDrivers)
            {
                if (displayDriverOption.DisplayMode.Invoke(displayMode))
                {
                    services.Add((IContentPartDisplayDriver)_serviceProvider.GetRequiredService(displayDriverOption.DisplayDriverType));
                }
            }
        }

        drivers = services.Count > 0 ? services.ToArray() : [];
        _displayModeDrivers[(partName, displayMode)] = drivers;

        return drivers;
    }

    public IList<IContentPartDisplayDriver> GetEditorDrivers(string partName, string editor)
    {
        var services = new List<IContentPartDisplayDriver>();

        if (_contentDisplayOptions.ContentPartOptions.TryGetValue(partName, out var contentPartDisplayOption))
        {
            foreach (var editorDriverOption in contentPartDisplayOption.EditorDrivers)
            {
                if (editorDriverOption.Editor.Invoke(editor))
                {
                    services.Add((IContentPartDisplayDriver)_serviceProvider.GetRequiredService(editorDriverOption.DisplayDriverType));
                }
            }
        }

        return services;
    }
}
