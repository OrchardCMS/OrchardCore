using Microsoft.AspNetCore.Mvc;
using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.JsonApi.ContentDisplay
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
