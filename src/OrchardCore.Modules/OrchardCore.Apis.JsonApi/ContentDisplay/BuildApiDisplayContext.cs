using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement.ModelBinding;

namespace OrchardCore.RestApis.ContentDisplay
{
    public class BuildApiDisplayContext
    {
        public BuildApiDisplayContext(ApiItem item, IUrlHelper urlHelper, IUpdateModel updater)
        {
            Item = item;
            UrlHelper = urlHelper;
            Updater = updater;
        }

        public ApiItem Item { get; set; }
        public IUrlHelper UrlHelper { get; set; }
        public IUpdateModel Updater { get; set; }
    }
}
