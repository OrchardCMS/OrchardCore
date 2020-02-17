using System;
using System.Threading.Tasks;

namespace OrchardCore.ContentManagement.Handlers
{
    public class ContentItemAspectContext
    {
        public ContentItem ContentItem { get; set; }
        public object Aspect { get; set; }

        /// <summary>
        /// Provides a value for a specific aspect type.
        /// </summary>
        public async Task<ContentItemAspectContext> ForAsync<TAspect>(Func<TAspect, Task> action) where TAspect : class
        {
            var aspect = Aspect as TAspect;

            if (aspect != null)
            {
                await action(aspect);
            }

            return this;
        }
    }
}
