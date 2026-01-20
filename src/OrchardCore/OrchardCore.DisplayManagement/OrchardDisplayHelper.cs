using Microsoft.AspNetCore.Http;

namespace OrchardCore.DisplayManagement.Razor
{
    internal class OrchardDisplayHelper : IOrchardDisplayHelper
    {
        public OrchardDisplayHelper(HttpContext context, IDisplayHelper displayHelper)
        {
            HttpContext = context;
            DisplayHelper = displayHelper;
        }

        public HttpContext HttpContext { get; set; }
        public IDisplayHelper DisplayHelper { get; set; }
    }
}
