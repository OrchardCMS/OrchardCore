using System.Threading.Tasks;

namespace OrchardCore.DisplayManagement.Theming
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
