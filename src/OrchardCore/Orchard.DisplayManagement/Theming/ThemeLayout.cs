using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Orchard.DisplayManagement.Theming
{
    public class ThemeLayout : Razor.RazorPage<dynamic>
    {
        public override async Task ExecuteAsync()
        {
            var body = RenderLayoutBody();
            this.ThemeLayout.Content.Add(body);
            Write(await DisplayAsync(ThemeLayout));
        }
    }
}
