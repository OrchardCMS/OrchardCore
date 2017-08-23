using Orchard.DisplayManagement.ModelBinding;

namespace Orchard.ContentManagement.Api.ContentDisplay
{
    public class BuildApiDisplayContext
    {
        public BuildApiDisplayContext(ApiItem item, IUpdateModel updater)
        {
            Item = item;
            Updater = updater;
        }

        public ApiItem Item { get; set; }
        public IUpdateModel Updater { get; set; }
    }
}
