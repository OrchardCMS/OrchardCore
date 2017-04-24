using System;

namespace Orchard.ContentManagement.Handlers
{
    public class ContentItemAspectContext
    {
        public ContentItem ContentItem { get; set; }
        public object Aspect { get; set; }

        /// <summary>
        /// Provides a value for a specific aspect type. 
        /// </summary>
        public ContentItemAspectContext For<TAspect>(Action<TAspect> action) where TAspect : class
        {
            var aspect = Aspect as TAspect;

            if (aspect != null)
            {
                action(aspect);
            }

            return this;
        }
    }

}
