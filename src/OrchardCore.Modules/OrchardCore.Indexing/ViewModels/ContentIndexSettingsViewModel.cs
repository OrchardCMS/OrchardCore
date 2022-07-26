using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace OrchardCore.Indexing.ViewModels
{
    public class ContentIndexSettingsViewModel
    {
        public ContentIndexSettings ContentIndexSettings { get; set; }

        /// <summary>
        /// TODO : Provide a way to find ContentElements that can be stored or not.
        /// Should be based on StorableAttribute and also should only be available for indexing Text.
        /// </summary>
        [BindNever]
        public bool IsStorable { get; set; } = true;
    }
}
