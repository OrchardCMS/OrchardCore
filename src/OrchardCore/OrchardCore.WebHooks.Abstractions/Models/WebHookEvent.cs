using System;

namespace OrchardCore.WebHooks.Abstractions.Models
{
    /// <summary>
    /// Defines a webhook event which can be subscribed to when registering a WebHook. 
    /// </summary>
    public class WebHookEvent
    {
        /// <summary>
        /// Gets or sets the unique name of the event, e.g. <c>article.updated</c>.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the event, e.g. <c>Article Updated</c>.
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets a description of the event.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category of the event. Used for grouping in the UI.
        /// </summary>
        public string Category { get; set; }

        public WebHookEvent(string name, string displayName = null, string description = null, string category = null)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            
            DisplayName = displayName;
            Description = description;
            Category = category;
        }
    }
}