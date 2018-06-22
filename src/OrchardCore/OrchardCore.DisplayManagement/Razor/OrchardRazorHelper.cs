using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.DisplayManagement.Razor
{
    public class OrchardRazorHelper
    {
        public OrchardRazorHelper(HttpContext context, IDisplayHelper displayHelper)
        {
            HttpContext = context;
            DisplayHelper = displayHelper;
        }

        public HttpContext HttpContext { get; set; }
        public IDisplayHelper DisplayHelper { get; set; }
    }
}
