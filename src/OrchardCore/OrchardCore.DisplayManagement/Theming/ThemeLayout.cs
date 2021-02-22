using System.Linq;
using System.Threading.Tasks;
using OrchardCore.DisplayManagement.Shapes;

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
                var content = ThemeLayout.Zones["Content"] as Shape;

                // Then is added to the Content zone of the Layout shape
                // Render Shapes in Content
                if (content != null)
                {
                    await content.AddAsync(body);

                    var htmlContent = await DisplayAsync(content);
                    content.Items.Clear();
                    await content.AddAsync(htmlContent);
                }

                foreach (var zone in ThemeLayout.Properties)
                {
                    // Is zone empty
                    if (zone.Value != null)
                    {
                        var shape = zone.Value as IShape;

                        // Render each layout zone
                        var htmlZone = await DisplayAsync(shape);
                        ThemeLayout.Properties[zone.Key] = htmlZone;
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
