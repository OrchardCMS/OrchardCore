using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
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
                // Then is added to the Content zone of the Layout shape
                ThemeLayout.Content.Add(body);

                //Render Shapes in Content 
                var htmlContent = await DisplayAsync(ThemeLayout.Content);

                var content = ((Shape)ThemeLayout.Content);

                foreach (var item in content.Items.ToArray())
                {
                    content.Remove(((IShape)item).Metadata.Name);
                }

                content.Add(htmlContent);

                var zoneKeys = ((Zones.ZoneHolding)ThemeLayout).Properties.Keys.ToArray();
                if(zoneKeys != null && zoneKeys.Count() > 0)
                {
                    foreach (var key in zoneKeys)
                    {
                        //Render each layout zone
                        var zone = ThemeLayout[key];
                        var htmlZone= DisplayAsync(zone);

                        foreach (var item in zone.Items.ToArray())
                        {
                            zone.Remove(((IShape)item).Metadata.Name);
                        }

                        zone.Add(htmlZone);
                    }
                }                
                // Finally we render the Layout Shape's HTML to the page's output
                var layout = await DisplayAsync(ThemeLayout);                
                Write(layout);
            }
            else
            {
                Write(body);
            }
        }
    }
}
