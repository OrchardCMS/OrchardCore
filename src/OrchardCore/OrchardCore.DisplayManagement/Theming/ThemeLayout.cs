using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;

namespace OrchardCore.DisplayManagement.Theming
{
    /// <summary>
    /// This class represents a precompiled _Layout.cshtml view that renders a
    /// Layout shape and the View's body in its Content zone.
    ///
    /// 1- Views look for any _ViewStart.cshtml
    /// 2- <see cref="ThemingViewsFeatureProvider"/> has registered <see cref="ThemeViewStart"/> as the top one
    /// 3- <see cref="ThemeViewStart"/> then set a special Layout filename as the default View's Layout.
    /// 4- <see cref="ThemingViewsFeatureProvider"/> has registered <see cref="ThemeLayout"/> for this special filename.
    /// 5- <see cref="ThemeLayout"/> evaluates the Body of the view, and renders a Layout shape with this Body in the Content zone.
    /// </summary>
    public class ThemeLayout : Razor.RazorPage<dynamic>
    {
        public override async Task ExecuteAsync()
        {
            // The View's body is rendered.
            var body = RenderLayoutBody();

            if (ThemeLayout != null)
            {
                // Then is added to the Content zone of the Layout shape.
                await ThemeLayout.Zones["Content"].AddAsync(body);

                // Pre-render all shapes and replace the zone content with it.
                ThemeLayout.Zones["Content"] = new PositionWrapper(await DisplayAsync(ThemeLayout.Zones["Content"]), "");

                // Render each layout zone.
                foreach (var zone in ThemeLayout.Properties.ToArray())
                {
                    if (zone.Value is IShape shape)
                    {
                        // Check if the shape is null or empty.
                        if (shape.IsNullOrEmpty())
                        {
                            continue;
                        }

                        // Check if the shape is pre-rendered.
                        if (shape is IHtmlContent)
                        {
                            continue;
                        }

                        ThemeLayout.Zones[zone.Key] = new PositionWrapper(await DisplayAsync(shape), "");
                    }
                }

                // Finally we render the Layout Shape's HTML to the page's output.
                Write(await DisplayAsync(ThemeLayout));
            }
            else
            {
                Write(body);
            }
        }
    }
}
