using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Orchard.DisplayManagement.Theming
{
    public class ThemeLayout : Razor.RazorPage<dynamic>
    {
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IUrlHelper Url { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IViewComponentHelper Component { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IJsonHelper Json { get; private set; }
        [Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public IHtmlHelper<dynamic> Html { get; private set; }

        public override async Task ExecuteAsync()
        {
            var body = RenderLayoutBody();
            this.ThemeLayout.Content.Add(body);
            Write(await DisplayAsync(ThemeLayout));
        }
    }
}
