using Microsoft.AspNet.Mvc;

namespace OrchardVNext.ContentManagement {
    public class ContentPart : IContent {
        public virtual ContentItem ContentItem { get; set; }

        /// <summary>
        /// The ContentItem's identifier.
        /// </summary>
        [HiddenInput(DisplayValue = false)]
        public int Id => ContentItem.Id;
    }
}