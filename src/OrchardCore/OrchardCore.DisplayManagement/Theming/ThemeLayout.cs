using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Zones;

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
            // The View's body is rendered
            var body = RenderLayoutBody();

            if (ThemeLayout != null)
            {
                // Then is added to the Content zone of the Layout shape
                ThemeLayout.Content.Add(body);

                // Render Shapes in Content
                if (ThemeLayout.Content is IShape content)
                {
                    var htmlContent = await DisplayAsync(content);
                    ThemeLayout.Content = htmlContent;
                }

                if (ThemeLayout is ZoneHolding layout)
                {
                    foreach (var zone in layout.Properties.ToArray())
                    {
                        if (zone.Value is IShape shape)
                        {
                            // Render each layout zone
                            var htmlZone = await DisplayAsync(shape);
                            layout.Properties[zone.Key] = htmlZone;
                        }
                    }
                }

                // Finally we render the Layout Shape's HTML to the page's output
                Write(await DisplayAsync(ThemeLayout));
            }
            else
            {
                Write(body);
            }
        }
    }
}
