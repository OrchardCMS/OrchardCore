using Microsoft.AspNetCore.Http;
using OrchardCore.DisplayManagement.Implementation;

namespace OrchardCore.DisplayManagement.Razor
{
    public interface IOrchardDisplayHelper : IOrchardHelper
    {
        IDisplayHelper DisplayHelper { get; }
    }

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
